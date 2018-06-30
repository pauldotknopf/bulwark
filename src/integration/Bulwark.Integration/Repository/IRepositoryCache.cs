using System.Threading.Tasks;

namespace Bulwark.Integration.Repository
{
    public interface IRepositoryCache
    {
        Task<IRepositorySession> GetDirectoryForRepo(string key);
    }
}