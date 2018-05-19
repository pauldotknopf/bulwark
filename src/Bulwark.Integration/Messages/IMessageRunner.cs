using System.Threading;
using System.Threading.Tasks;

namespace Bulwark.Integration.Messages
{
    public interface IMessageRunner
    {
        Task Run(CancellationToken token);

        void RegisterMessage<T>() where T : class;
    }
}