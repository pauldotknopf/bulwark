using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Bulwark.Integration.Messages.Impl
{
    public class InMemoryMessageSender : IMessageSender
    {
        readonly IServiceScopeFactory _serviceScopeFactory;

        public InMemoryMessageSender(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        public Task Send<T>(T message) where T : class
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                // NOTE: this task is purposefully not awaited on so that it happens in the background.
                scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>().Handle(message);
            }
            return Task.CompletedTask;
        }
    }
}