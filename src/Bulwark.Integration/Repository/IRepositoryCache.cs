using System.Threading.Tasks;
using Bulwark.Integration.GitLab;

namespace Bulwark.Integration.Repository
{
    public interface IRepositoryCache
    {
        Task<IRepositorySession> GetDirectoryForRepo(string key);
    }
}