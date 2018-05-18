using Bulwark.Integration.GitLab.Hooks;
using Bulwark.Integration.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Bulwark.Integration.GitLab
{
    public static class Services
    {
        public static void Register(IServiceCollection services)
        {
            services.AddScoped<IMessageHandler<MergeRequestEvent>, MergeRequestEventHandler>();
            services.AddSingleton<Api.IGitLabApi, Api.Impl.GitLabApi>();
        }
    }
}