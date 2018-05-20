using System;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bulwark.Integration.Messages.Impl
{
    public class RabbitMqMessageSender : IMessageSender, IMessageRunner
    {
        readonly IOptions<MessageQueueOptions> _messageQueueOptions;
        readonly IOptions<MessageTypeOptions> _messageTypeOptions;
        readonly IServiceScopeFactory _serviceScopeFactory;
        readonly ILogger<RabbitMqMessageSender> _logger;
        readonly IBusControl _bus;
        
        public RabbitMqMessageSender(
            IOptions<MessageQueueOptions> messageQueueOptions,
            IOptions<MessageTypeOptions> messageTypeOptions,
            IServiceScopeFactory serviceScopeFactory,
            ILoggerFactory loggerFactory)
        {
            _messageQueueOptions = messageQueueOptions;
            _messageTypeOptions = messageTypeOptions;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = loggerFactory.CreateLogger<RabbitMqMessageSender>();
            
            _bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var value = messageQueueOptions.Value;
                sbc.Host(new Uri($"rabbitmq://{value.RabbitMqHost}"), h =>
                {
                    h.Username(value.RabbitMqUsername);
                    h.Password(value.RabbitMqPassword);
                });
            });
        }

        public async Task Send<T>(T message) where T : class
        {
            _logger.LogInformation($"Pushing message {typeof(T).Name}");

            var endpoint = await _bus.GetSendEndpoint(new Uri($"rabbitmq://{_messageQueueOptions.Value.RabbitMqHost}/work_queue"));
            await endpoint.Send(message);
        }

        public async Task<IMessageRunnerSession> Run()
        {
            _logger.LogWarning("Starting RabbitMQ listener");
            
            var receiveBus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri($"rabbitmq://{_messageQueueOptions.Value.RabbitMqHost}"), h =>
                {
                    h.Username(_messageQueueOptions.Value.RabbitMqUsername);
                    h.Password(_messageQueueOptions.Value.RabbitMqPassword);
                });

                sbc.ReceiveEndpoint(host, "work_queue", ep =>
                {
                    foreach (var type in _messageTypeOptions.Value.Types)
                    {
                        var method = typeof(RabbitMqMessageSender).GetMethod("RegisterHandler", BindingFlags.Instance | BindingFlags.NonPublic);
                        // ReSharper disable once PossibleNullReferenceException
                        var genericMethod = method.MakeGenericMethod(type);
                        genericMethod.Invoke(this, new object[] {ep});
                    }
                });
            });

            await receiveBus.StartAsync();
            
            return new RabbitRunner(_logger, receiveBus);
        }

        // ReSharper disable once UnusedMember.Local
        private void RegisterHandler<T>(IRabbitMqReceiveEndpointConfigurator ep) where T : class
        {
            ep.Handler<T>(async context =>
            {
                _logger.LogInformation($"Handling message {typeof(T).Name}");
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    await scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>().Handle(context.Message);
                }
            });
        }

        class RabbitRunner : IMessageRunnerSession
        {
            readonly ILogger<RabbitMqMessageSender> _logger;
            readonly IBusControl _bus;

            public RabbitRunner(ILogger<RabbitMqMessageSender> logger, IBusControl bus)
            {
                _logger = logger;
                _bus = bus;
            }
            
            public async Task DisposeAsync()
            {
                _logger.LogInformation("Stopping RabbitMQ listener");
                await _bus.StopAsync();
            }
        }
    }
}