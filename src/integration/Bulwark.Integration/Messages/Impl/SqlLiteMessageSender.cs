using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServiceStack.Data;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace Bulwark.Integration.Messages.Impl
{
    public class SqlLiteMessageSender : IMessageSender, IMessageRunner
    {
        readonly IServiceScopeFactory _serviceScopeFactory;
        readonly IDbConnectionFactory _dbFactory;
        readonly ILogger<SqlLiteMessageSender> _logger;
        
        public SqlLiteMessageSender(
            IOptions<MessageQueueOptions> messageQueueOptions,
            ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            var messageQueueOptionsValue = messageQueueOptions.Value;
            
            _dbFactory = new OrmLiteConnectionFactory(
                messageQueueOptionsValue.SqlLiteDBLocation,  
                SqliteDialect.Provider);
            _logger = loggerFactory.CreateLogger<SqlLiteMessageSender>();
            
            using (var db = _dbFactory.Open())
            {
                db.CreateTableIfNotExists<Message>();
            }
        }
        
        public async Task Send<T>(T message) where T : class
        {
            using (var connection = _dbFactory.OpenDbConnection())
            {
                connection.Open();
                await connection.InsertAsync(new Message
                {
                    ScheduleOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Type = $"{message.GetType().FullName}, {message.GetType().Assembly.FullName}",
                    Content = JsonConvert.SerializeObject(message)
                });
            }
        }

        public Task<IMessageRunnerSession> Run()
        {
            return Task.FromResult<IMessageRunnerSession>(new RunnerSession(_dbFactory, _logger, _serviceScopeFactory));
        }
        
        class Message
        {
            [AutoIncrement]
            public int Id { get; set; }
            
            public long ScheduleOn { get; set; }
            
            public string Type { get; set; }
            
            public string Content { get; set; }
        }
        
        class RunnerSession : IMessageRunnerSession
        {
            readonly IDbConnectionFactory _dbFactory;
            readonly ILogger<SqlLiteMessageSender> _logger;
            readonly IServiceScopeFactory _serviceScopeFactory;
            readonly Thread _thread;
            readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
            readonly MethodInfo _processMessageMethod;
            
            public RunnerSession(IDbConnectionFactory dbFactory,
                ILogger<SqlLiteMessageSender> logger,
                IServiceScopeFactory serviceScopeFactory)
            {
                _processMessageMethod = typeof(RunnerSession).GetMethod("ProcessMessage", BindingFlags.Instance | BindingFlags.NonPublic);
                _dbFactory = dbFactory;
                _logger = logger;
                _serviceScopeFactory = serviceScopeFactory;
                _thread = new Thread(ThreadFun);
                _thread.Start();
            }

            private void ThreadFun()
            {
                try
                {
                    while (!_cancellationToken.IsCancellationRequested)
                    {
                        Message next = null;
                        using (var connection = _dbFactory.CreateDbConnection())
                        {
                            connection.Open();
                            var query = connection.From<Message>().OrderBy(x => x.ScheduleOn)
                                .Limit(1);
                            next = connection.LoadSelect(query)
                                .FirstOrDefault();
                        }
                        
                        if (next == null)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        _logger.LogInformation("Got message from database.");
                        
                        try
                        {
                            var type = Type.GetType(next.Type);
                            if (type == null) throw new Exception($"Invalid type {next.Type}");

                            _logger.LogInformation($"Processing message {type.Name}");
                            
                            var method = _processMessageMethod.MakeGenericMethod(type);
                            method.Invoke(this, new object[] {next.Content});

                            // Processed
                            using (var connection = _dbFactory.CreateDbConnection())
                            {
                                connection.Open();
                                connection.Delete(next);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing message.");
                            // This will push it back, causing newer messages to get processed.
                            // Otherwise, only failed messages will be processed, leaving new/valid ones
                            // stuck in line.
                            next.ScheduleOn = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(5))
                                .ToUnixTimeSeconds();
                            using (var connection = _dbFactory.CreateDbConnection())
                            {
                                connection.Open();
                                connection.Update(next);
                            }
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