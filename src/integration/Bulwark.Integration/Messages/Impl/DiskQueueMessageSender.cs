using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiskQueue;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bulwark.Integration.Messages.Impl
{
    public class DiskQueueMessageSender : IMessageSender, IMessageRunner
    {
        readonly IServiceScopeFactory _serviceScopeFactory;
        readonly IPersistentQueue _queue;
        readonly ILogger<DiskQueueMessageSender> _logger;
        
        public DiskQueueMessageSender(IOptions<MessageQueueOptions> messageQueueOptions,
            ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = loggerFactory.CreateLogger<DiskQueueMessageSender>();
            _queue = new PersistentQueue(messageQueueOptions.Value.DiskQueueLocation);
        }
        
        public Task Send<T>(T message) where T : class
        {
            using (var session = _queue.OpenSession())
            {
                session.Enqueue(new Message
                {
                    Type = $"{message.GetType().FullName}, {message.GetType().Assembly.FullName}",
                    Content = JsonConvert.SerializeObject(message)
                }.ToBytes());
                session.Flush();
            }

            return Task.CompletedTask;
        }

        public Task<IMessageRunnerSession> Run()
        {
            return Task.FromResult<IMessageRunnerSession>(new RunnerSession(_queue, _logger, _serviceScopeFactory));
        }

        class Message
        {
            public string Type { get; set; }
            
            public string Content { get; set; }

            public byte[] ToBytes()
            {
                return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
            }
        }

        class RunnerSession : IMessageRunnerSession
        {
            readonly IPersistentQueue _queue;
            readonly ILogger<DiskQueueMessageSender> _logger;
            readonly IServiceScopeFactory _serviceScopeFactory;
            readonly Thread _thread;
            readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
            readonly MethodInfo _processMessageMethod;
            
            public RunnerSession(IPersistentQueue queue,
                ILogger<DiskQueueMessageSender> logger,
                IServiceScopeFactory serviceScopeFactory)
            {
                _processMessageMethod = typeof(RunnerSession).GetMethod("ProcessMessageInternal", BindingFlags.Instance | BindingFlags.NonPublic);
                _queue = queue;
                _logger = logger;
                _serviceScopeFactory = serviceScopeFactory;
                _thread = new Thread(ThreadFun);
                _thread.Start();
            }

            private void ThreadFun()
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    using (var session = _queue.OpenSession())
                    {
                        var data = session.Dequeue();
                        if (data == null)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        try
                        {
                            ProcessMessage(data);
                            session.Flush();
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError(ex, "Error processing message.");
                        }
                    }
                }
            }

            private void ProcessMessage(byte[] data)
            {
                var message = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(data));
                var type = Type.GetType(message.Type);
                if(type == null) throw new Exception($"Invalid type {message.Type}");
                var method = _processMessageMethod.MakeGenericMethod(type);
                method.Invoke(this, new object[]{ message.Content });
            }

            // ReSharper disable once UnusedMember.Local
            private void ProcessMessageInternal<T>(string data) where T : class
            {
                var message = JsonConvert.DeserializeObject<T>(data);
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>()
                        .Handle(message)
                        .GetAwaiter()
                        .GetResult();
                }
            }
            
            public Task DisposeAsync()
            {
                _cancellationToken.Cancel();
                _thread.Join();
                return Task.CompletedTask;
            }
        }
    }
}