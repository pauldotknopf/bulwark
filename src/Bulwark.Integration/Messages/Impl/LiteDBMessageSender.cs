using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Bulwark.Integration.Messages.Impl
{
    public class LiteDBMessageSender : IMessageSender, IMessageRunner
    {
        readonly IServiceScopeFactory _serviceScopeFactory;
        readonly LiteDatabase _database;
        readonly ILogger<LiteDBMessageSender> _logger;
        
        public LiteDBMessageSender(IOptions<MessageQueueOptions> messageQueueOptions,
            ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            var messageOptionsValue = messageQueueOptions.Value;
            if (string.IsNullOrEmpty(messageOptionsValue.LiteDBLocation))
            {
                throw new Exception("You must provide a location to a LiteDB database.");
            }
            _logger = loggerFactory.CreateLogger<LiteDBMessageSender>();
            _database = new LiteDatabase(messageOptionsValue.LiteDBLocation);
        }
        
        public Task Send<T>(T message) where T : class
        {
            _logger.LogInformation($"Sending message {typeof(T).Namespace}");
            Console.WriteLine($"Sending message {typeof(T).Namespace}");
            var messages = _database.GetCollection<Message>("messages");
            messages.Insert(new Message
            {
                ScheduleOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Type = $"{message.GetType().FullName}, {message.GetType().Assembly.FullName}",
                Content = JsonConvert.SerializeObject(message)
            });
            return Task.CompletedTask;
        }

        public Task<IMessageRunnerSession> Run()
        {
            return Task.FromResult<IMessageRunnerSession>(new RunnerSession(_database, _logger, _serviceScopeFactory));
        }
        
        class Message
        {
            public int Id { get; set; }
            
            public long ScheduleOn { get; set; }
            
            public string Type { get; set; }
            
            public string Content { get; set; }
        }

         class RunnerSession : IMessageRunnerSession
        {
            readonly LiteDatabase _database;
            readonly ILogger<LiteDBMessageSender> _logger;
            readonly IServiceScopeFactory _serviceScopeFactory;
            readonly Thread _thread;
            readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
            readonly MethodInfo _processMessageMethod;
            
            public RunnerSession(LiteDatabase database,
                ILogger<LiteDBMessageSender> logger,
                IServiceScopeFactory serviceScopeFactory)
            {
                _processMessageMethod = typeof(RunnerSession).GetMethod("ProcessMessage", BindingFlags.Instance | BindingFlags.NonPublic);
                _database = database;
                _logger = logger;
                _serviceScopeFactory = serviceScopeFactory;
                _thread = new Thread(ThreadFun);
                _thread.Start();
            }

            private void ThreadFun()
            {
                try
                {
                    var collection = _database.GetCollection<Message>("messages");
                    while (!_cancellationToken.IsCancellationRequested)
                    {
                        var next = collection.FindOne(Query.All("ScheduleOn"));
                        if (next == null)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        _logger.LogInformation("Got message from database.");
                        Console.WriteLine("Got message from database.");
                        
                        try
                        {
                            var type = Type.GetType(next.Type);
                            if (type == null) throw new Exception($"Invalid type {next.Type}");

                            _logger.LogInformation($"Processing message {type.Name}");
                            Console.WriteLine($"Processing message {type.Name}");
                            
                            var method = _processMessageMethod.MakeGenericMethod(type);
                            method.Invoke(this, new object[] {next.Content});

                            // Processed
                            collection.Delete(next.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing message.");
                            // This will push it back, causing newer messages to get processed.
                            // Otherwise, only failed messages will be processed, leaving new/valid ones
                            // stuck in line.
                            next.ScheduleOn = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(5))
                                .ToUnixTimeSeconds();
                            collection.Update(next);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Message processer thread died.");
                }
            }

            // ReSharper disable once UnusedMember.Local
            private void ProcessMessage<T>(string data) where T : class
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