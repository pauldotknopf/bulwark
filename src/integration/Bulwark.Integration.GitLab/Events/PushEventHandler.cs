using System.Diagnostics;
using System.Threading.Tasks;
using Bulwark.Integration.GitLab.Api;
using Bulwark.Integration.GitLab.Api.Requests;
using Bulwark.Integration.GitLab.Api.Types;
using Bulwark.Integration.Messages;

namespace Bulwark.Integration.GitLab.Events
{
    public class PushEventHandler : IMessageHandler<PushEvent>
    {
        readonly IGitLabApi _api;
        readonly IMessageSender _messageSender;

        public PushEventHandler(IGitLabApi api,
            IMessageSender messageSender)
        {
            _api = api;
            _messageSender = messageSender;
        }
        
        public async Task Handle(PushEvent message)
        {
            await Task.Delay(3000);
            
            Debug.WriteLine(message);

            var targetBranch = message.Push.Ref;
            if (targetBranch.StartsWith("refs/heads/"))
                targetBranch = targetBranch.Substring("refs/heads/".Length);
            
            // Get all open merge requests that are merging into this branch that was pushed.
            var mergeRequests = await _api.GetMergeRequests(new MergeRequestsRequest
            {
                ProjectId = message.Push.Project.Id,
                TargetBranch = targetBranch,
                State = MergeRequestState.Opened
            });
            
            // Send a message to re-run each merge requests
            foreach (var mergeRequest in mergeRequests)
            {
                await _messageSender.Send(new UpdateMergeRequestEvent
                {
                    ProjectId = mergeRequest.ProjectId,
                    MergeRequestIid = mergeRequest.Iid
                });
            }
        }
    }
}