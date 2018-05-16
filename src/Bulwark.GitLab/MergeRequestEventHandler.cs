using System;
using System.Threading.Tasks;
using Bulwark.Integration.Messages;

namespace Bulwark.GitLab
{
    public class MergeRequestEventHandler : IMessageHandler<MergeRequestEvent>
    {
        public async Task Handle(MergeRequestEvent message)
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    }
}