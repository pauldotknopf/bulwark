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

        public async Task Run(CancellationToken token)
        {
            // Get all registered handlers
            _server.Start();
            
            // Wait for a cancellation request
            var tcs = new TaskCompletionSource<object>();
            IDisposable subscription = null;
            subscription = token.Register(() =>
            {
                tcs.SetResult(null);
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once AccessToModifiedClosure
                subscription.Dispose();
            });
            await tcs.Task;
            
            // We are stopping, shutdown the processing of messages.
            _server.Stop();
            _server.WaitForWorkersToStop();
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
    }
}