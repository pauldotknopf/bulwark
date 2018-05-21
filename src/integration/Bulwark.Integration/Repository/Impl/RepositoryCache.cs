using System.IO;
using System.Threading.Tasks;
using Bulwark.Integration.GitLab;
using LibGit2Sharp;
using Microsoft.Extensions.Options;

namespace Bulwark.Integration.Repository.Impl
{
    public class RepositoryCache : IRepositoryCache
    {
        readonly RepositoryCacheOptions _options;
        
        public RepositoryCache(IOptions<RepositoryCacheOptions> options)
        {
            _options = options.Value;
        }
        
        public async Task<IRepositorySession> GetDirectoryForRepo(string key)
        {
            // TODO: Add some locks around this.
            Session session = null;
            
            await Task.Run(() =>
            {
                var root = Path.Combine(_options.RepositoryCacheLocation, "gitlab");
                if (!Directory.Exists(root))
                    Directory.CreateDirectory(root);
                var directory = Path.Combine(root, $"repo-{key}");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    LibGit2Sharp.Repository.Init(directory);
                }
                session = new Session(new LibGit2Sharp.Repository(directory));
            });

            return session;
        }
        
        class Session : IRepositorySession
        {
            public Session(LibGit2Sharp.Repository repo)
            {
                Repository = repo;
            }
            
            public IRepository Repository { get; }
            
            public void Dispose()
            {
                Repository.Dispose();
            }
        }
    }
}