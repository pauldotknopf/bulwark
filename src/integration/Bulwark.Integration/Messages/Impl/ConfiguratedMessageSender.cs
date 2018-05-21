using System;
using System.Threading;
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
            IOptions<MessageTypeOptions> messageTypesOptions,
            IOptions<MessageQueueOptions> messageQueueOptions,
            ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory)
        {
            var logger = loggerFactory.CreateLogger<ConfiguratedMessageSender>();
            var value = messageQueueOptions.Value;
            switch (value.Type)
            {
                case MessageQueueOptions.Types.MessageQueueType.InMemory:
                    logger.LogWarning("Don't use the in-memory message handler in production. Try RabbitMQ, or send a PR for something else.");
                    var inMemory = new InMemoryMessageSender(serviceScopeFactory, loggerFactory);
                    _innerSender = inMemory;
                    _innerRunner = inMemory;
                    break;
                case MessageQueueOptions.Types.MessageQueueType.RabbitMQ:
                    var rabbitMq = new RabbitMqMessageSender(messageQueueOptions, messageTypesOptions, serviceScopeFactory, loggerFactory);
                    _innerSender = rabbitMq;
                    _innerRunner = rabbitMq;
                    break;
                case MessageQueueOptions.Types.MessageQueueType.DiskQueue:
                    var diskQueue = new DiskQueueMessageSender(messageQueueOptions, loggerFactory, serviceScopeFactory);
                    _innerSender = diskQueue;
                    _innerRunner = diskQueue;
                    break;
                case MessageQueueOptions.Types.MessageQueueType.LiteDB:
                    var liteDb = new LiteDBMessageSender(messageQueueOptions, loggerFactory, serviceScopeFactory);
                    _innerSender = liteDb;
                    _innerRunner = liteDb;
                    break;
                case MessageQueueOptions.Types.MessageQueueType.Sqlite:
                    var sqlite = new SqlLiteMessageSender(messageQueueOptions, loggerFactory, serviceScopeFactory);
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