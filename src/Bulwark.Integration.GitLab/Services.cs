using Bulwark.Integration.GitLab.Events;
using Bulwark.Integration.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bulwark.Integration.GitLab
{
    public static class Services
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IMessageHandler<MergeRequestEvent>, MergeRequestEventHandler>();
            services.AddScoped<IMessageHandler<PushEvent>, PushEventHandler>();
            services.AddScoped<IMessageHandler<UpdateMergeRequestEvent>, UpdateMergeRequestEventHandler>();
            services.AddSingleton<Api.IGitLabApi, Api.Impl.GitLabApi>();
            services.AddSingleton<IMergeRequestProcessor, Impl.MergeRequestProcessor>();
            services.Configure<GitLabOptions>(configuration.GetSection("GitLab"));
        }
    }
}