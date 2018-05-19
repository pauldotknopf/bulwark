using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bulwark.Integration.GitLab.Api;
using Bulwark.Integration.GitLab.Api.Requests;
using Bulwark.Integration.GitLab.Api.Types;
using Bulwark.Integration.Messages;
using Bulwark.Integration.Repository;
using Bulwark.Strategy.CodeOwners;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace Bulwark.Integration.GitLab.Events
{
    public class MergeRequestEventHandler : IMessageHandler<MergeRequestEvent>
    {
        readonly IMergeRequestProcessor _mergeRequestProcessor;

        public MergeRequestEventHandler(IMergeRequestProcessor mergeRequestProcessor)
        {
            _mergeRequestProcessor = mergeRequestProcessor;
        }
        
        public Task Handle(MergeRequestEvent message)
        {
            return _mergeRequestProcessor.ProcessMergeRequest(
                message.MergeRequest.ObjectAttributes.TargetProjectId,
                message.MergeRequest.ObjectAttributes.Iid);
        }
    }
}