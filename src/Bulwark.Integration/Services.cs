using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bulwark.Integration
{
    public static class Services
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<Messages.Impl.ConfiguratedMessageSender>();
            services.AddSingleton<Messages.IMessageRunner>(context => context.GetRequiredService<Messages.Impl.ConfiguratedMessageSender>());
            services.AddSingleton<Messages.IMessageSender>(context => context.GetRequiredService<Messages.Impl.ConfiguratedMessageSender>());
            services.AddSingleton<Repository.IRepositoryCache, Repository.Impl.RepositoryCache>();
            services.Configure<RepositoryCacheOptions>(configuration.GetSection("RepositoryCache"));
            services.Configure<MessageQueueOptions>(configuration.GetSection("MessageQueue"));
        }
    }
}