using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using ServiceStack.RabbitMq;

namespace Bulwark.Integration.Messages.Impl
{
    public class RabbitMqMessageSender : IMessageSender, IMessageRunner
    {
        readonly IServiceScopeFactory _serviceScopeFactory;
        readonly RabbitMqServer _server;
        
        public RabbitMqMessageSender(string serverUrl,
            IServiceScopeFactory serviceScopeFactory)
        {
            if(string.IsNullOrEmpty(serverUrl)) throw new Exception("You must provide a rabbitmq server url.");
            _serviceScopeFactory = serviceScopeFactory;
            _server = new RabbitMqServer(serverUrl);
        }
        
        public Task Send<T>(T message) where T : class
        {
            return Task.Run(() =>
            {
                using (var client = _server.CreateMessageQueueClient())
                {
                    client.Publish(message);
                }
            });
        }

        public IDisposable Run()
        {
            // Get all registered handlers
            _server.Start();
            
            return new RabbitRunner(_server);
        }

        public void RegisterMessage<T>() where T : class
        {
            _server.RegisterHandler<T>(message =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var task = scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>().Handle(message.GetBody());
                    task.GetAwaiter().GetResult();
                }

                return null;
            });
        }

        class RabbitRunner : IDisposable
        {
            private readonly RabbitMqServer _server;

            public RabbitRunner(RabbitMqServer server)
            {
                _server = server;
            }
            
            public void Dispose()
            {
                // We are stopping, shutdown the processing of messages, wait for finish.
                _server.Stop();
                _server.WaitForWorkersToStop();
            }
        }
    }
}