using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bulwark.Integration.Messages
{
    public interface IMessageRunner
    {
        IDisposable Run();

        void RegisterMessage<T>() where T : class;
    }
}