using System;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bulwark.Integration.Messages.Impl
{
    public class RabbitMqMessageSender : IMessageSender, IMessageRunner
    {
        readonly MessageQueueOptions _messageQueueOptions;
        readonly IServiceScopeFactory _serviceScopeFactory;
        readonly ILogger<RabbitMqMessageSender> _logger;
        readonly IBusControl _bus;
        readonly MethodInfo _processMessageMethod;
        
        public RabbitMqMessageSender(
            IOptions<MessageQueueOptions> messageQueueOptions,
            IServiceScopeFactory serviceScopeFactory,
            ILoggerFactory loggerFactory)
        {
            _processMessageMethod = typeof(RabbitMqMessageSender).GetMethod("ProcessMessage", BindingFlags.Instance | BindingFlags.NonPublic);
            _messageQueueOptions = messageQueueOptions.Value;
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

            var endpoint = await _bus.GetSendEndpoint(new Uri($"rabbitmq://{_messageQueueOptions.RabbitMqHost}/work_queue"));
            await endpoint.Send(new Message
            {
                Type = $"{message.GetType().FullName}, {message.GetType().Assembly.FullName}",
                Content = JsonConvert.SerializeObject(message)
            });
        }

        public async Task<IMessageRunnerSession> Run()
        {
            _logger.LogWarning("Starting RabbitMQ listener");
            
            var receiveBus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri($"rabbitmq://{_messageQueueOptions.RabbitMqHost}"), h =>
                {
                    h.Username(_messageQueueOptions.RabbitMqUsername);
                    h.Password(_messageQueueOptions.RabbitMqPassword);
                });

                sbc.ReceiveEndpoint(host, "work_queue", ep =>
                {
                    ep.Handler<Message>(async context =>
                    {
                        var type = Type.GetType(context.Message.Type);
                        if (type == null) throw new Exception($"Invalid type {context.Message.Type}");

                        _logger.LogInformation($"Processing message {type.Name}");
                        
                        var method = _processMessageMethod.MakeGenericMethod(type);
                        var result = (Task)method.Invoke(this, new object[] {context.Message.Content});
                        
                        await result;
                    });
                });
            });

            await receiveBus.StartAsync();
            
            return new RabbitRunner(_logger, receiveBus);
        }
        
        // ReSharper disable once UnusedMember.Local
        private async Task ProcessMessage<T>(string data) where T : class
        {
            var message = JsonConvert.DeserializeObject<T>(data);
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>()
                    .Handle(message);
            }
        }

        class Message
        {
            public string Type { get; set; }
            
            public string Content { get; set; }
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