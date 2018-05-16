using Microsoft.Extensions.DependencyInjection;

namespace Bulwark
{
    public static class Services
    {
        public static void Register(IServiceCollection services)
        {
            services.AddSingleton<Strategy.CodeOwners.ICodeOwnersWalker, Strategy.CodeOwners.Impl.CodeOwnersWalker>();
            services.AddSingleton<Strategy.CodeOwners.ICodeOwnersChangeset, Strategy.CodeOwners.Impl.CodeOwnersChangeset>();
            services.AddSingleton<Strategy.CodeOwners.ICodeOwnersParser, Strategy.CodeOwners.Impl.CodeOwnersParser>();
        }
    }
}