using System.Threading.Tasks;
using Bulwark.Integration.Messages;

namespace Bulwark.Integration.GitLab.Events
{
    public class UpdateMergeRequestEventHandler : IMessageHandler<UpdateMergeRequestEvent>
    {
        readonly IMergeRequestProcessor _mergeRequestProcessor;

        public UpdateMergeRequestEventHandler(IMergeRequestProcessor mergeRequestProcessor)
        {
            _mergeRequestProcessor = mergeRequestProcessor;
        }
        
        public Task Handle(UpdateMergeRequestEvent message)
        {
            return _mergeRequestProcessor.ProcessMergeRequest(message.ProjectId, message.MergeRequestIid);
        }
    }
}