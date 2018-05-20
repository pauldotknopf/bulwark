using System;
using System.Threading;
using System.Threading.Tasks;
using Bulwark.Integration.Messages;
using Microsoft.Extensions.Hosting;

namespace Bulwark.Integration.WebHook
{
    public class WorkerService : IHostedService
    {
        readonly IMessageRunner _messageRunner;
        IDisposable _messageRunnerSession;
        readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1,1);
        
        public WorkerService(IMessageRunner messageRunner)
        {
            _messageRunner = messageRunner;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                if (_messageRunnerSession != null) return;
                _messageRunnerSession = _messageRunner.Run();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                if (_messageRunnerSession == null) return;
                _messageRunnerSession.Dispose();
                _messageRunnerSession = null;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}