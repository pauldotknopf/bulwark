using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bulwark.Integration.Messages.Impl
{
    public class ConfiguratedMessageSender : IMessageSender, IMessageRunner
    {
        readonly IMessageSender _innerSender;
        readonly IMessageRunner _innerRunner;

        public ConfiguratedMessageSender(
            IOptions<MessageQueueOptions> messageQueueOptions,
            ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory)
        {
            var logger = loggerFactory.CreateLogger<ConfiguratedMessageSender>();
            var value = messageQueueOptions.Value;
            switch (value.Type)
            {
                case MessageQueueOptions.Types.MessageQueueType.InMemory:
                    logger.LogWarning("Don't use the in-memory message handler in production. Try RabbitMQ, Sqlite, or send a PR for something else.");
                    var inMemory = new InMemoryMessageSender(serviceScopeFactory, loggerFactory);
                    _innerSender = inMemory;
                    _innerRunner = inMemory;
                    break;
                case MessageQueueOptions.Types.MessageQueueType.RabbitMq:
                    var rabbitMq = new RabbitMqMessageSender(messageQueueOptions, serviceScopeFactory, loggerFactory);
                    _innerSender = rabbitMq;
                    _innerRunner = rabbitMq;
                    break;
                case MessageQueueOptions.Types.MessageQueueType.Sqlite:
                    var sqlite = new SqliteMessageSender(messageQueueOptions, loggerFactory, serviceScopeFactory);
                    _innerSender = sqlite;
                    _innerRunner = sqlite;
                    break;
                default:
                    throw new Exception($"Unknow message queue type {value.Type}");
            }
        }
        
        public Task Send<T>(T message) where T : class
        {
            return _innerSender.Send(message);
        }

        public Task<IMessageRunnerSession> Run()
        {
            return _innerRunner.Run();
        }
    }
}