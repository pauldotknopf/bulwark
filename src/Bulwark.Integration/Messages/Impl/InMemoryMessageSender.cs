﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bulwark.Integration.Messages.Impl
{
    public class InMemoryMessageSender : IMessageSender, IMessageRunner
    {
        readonly IServiceScopeFactory _serviceScopeFactory;
        readonly ILogger<InMemoryMessageSender> _logger;

        public InMemoryMessageSender(IServiceScopeFactory serviceScopeFactory,
            ILoggerFactory loggerFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = loggerFactory.CreateLogger<InMemoryMessageSender>();
        }
        
        public Task Send<T>(T message) where T : class
        {
            var scope = _serviceScopeFactory.CreateScope();
            
            // NOTE: this task is purposefully not awaited on so that it happens in the background.
            Task.Run(async () =>
            {
                try
                {
                    await scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>().Handle(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending message");
                }
                finally
                {
                    scope.Dispose();
                }
            });
            
            return Task.CompletedTask;
        }

        public IDisposable Run()
        {
            Console.WriteLine("Starting in memory runner...");
            return new InMemoryRunner();
        }

        public void RegisterMessage<T>() where T : class
        {
            // Not needed.
        }

        class InMemoryRunner : IDisposable
        {
            public void Dispose()
            {
                Console.WriteLine("Finished in memory runner");
            }
        }
    }
}