using Microsoft.Extensions.DependencyInjection;

namespace Bulwark.Integration
{
    public static class Services
    {
        public static void Register(IServiceCollection services)
        {
            services.AddSingleton<Messages.IMessageSender, Messages.Impl.InMemoryMessageSender>();
            services.AddSingleton<Repository.IRepositoryCache, Repository.Impl.RepositoryCache>();
        }
    }
}