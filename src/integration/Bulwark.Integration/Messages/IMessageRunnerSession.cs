using System.Threading.Tasks;

namespace Bulwark.Integration.Messages
{
    public interface IMessageRunnerSession
    {
        Task DisposeAsync();
    }
}