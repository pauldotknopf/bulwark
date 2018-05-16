using Bulwark.Integration.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Bulwark.Integration.GitLab
{
    public static class Services
    {
        public static void Register(IServiceCollection services)
        {
            services.AddScoped<IMessageHandler<MergeRequestEvent>, MergeRequestEventHandler>();
        }
    }
}