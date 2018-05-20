using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bulwark.Integration.Messages
{
    public interface IMessageRunner
    {
        Task<IMessageRunnerSession> Run();
    }
}