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
            IOptions<MessageQueueOptions> options,
            ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory)
        {
            var optionsValue = options.Value;
            switch (optionsValue.MessageQueueType)
            {
                    case MessageQueueOptions.Types.MessageQueueType.InMemory:
                        var inMemory = new InMemoryMessageSender(serviceScopeFactory, loggerFactory);
                        _innerSender = inMemory;
                        _innerRunner = inMemory;
                        break;
                    case MessageQueueOptions.Types.MessageQueueType.RabbitMq:
                        var rabbitMq = new RabbitMqMessageSender(optionsValue.RabbitMqServerUrl, serviceScopeFactory);
                        _innerSender = rabbitMq;
                        _innerRunner = rabbitMq;
                        break;
                    default:
                        throw new Exception($"Unknow message queue type {optionsValue.MessageQueueType}");
            }
        }
        
        public Task Send<T>(T message) where T : class
        {
            return _innerSender.Send(message);
        }

        public IDisposable Run()
        {
            return _innerRunner.Run();
        }

        public void RegisterMessage<T>() where T : class
        {
            _innerRunner.RegisterMessage<T>();
        }
    }
}