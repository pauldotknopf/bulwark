using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bulwark.Integration.GitLab.Api;
using Bulwark.Integration.GitLab.Hooks;
using Bulwark.Integration.Messages;
using Bulwark.Integration.Repository;
using Bulwark.Strategy.CodeOwners;
using LibGit2Sharp;
using Commit = LibGit2Sharp.Commit;

namespace Bulwark.Integration.GitLab
{
    public class MergeRequestEventHandler : IMessageHandler<MergeRequestEvent>
    {
        readonly IRepositoryCache _repositoryCache;
        readonly ICodeOwnersChangeset _changeset;
        readonly IGitLabApi _api;

        public MergeRequestEventHandler(IRepositoryCache repositoryCache,
            ICodeOwnersChangeset changeset,
            IGitLabApi api)
        {
            _repositoryCache = repositoryCache;
            _changeset = changeset;
            _api = api;
        }
        
        public async Task Handle(MergeRequestEvent message)
        {
            await Task.Delay(4);
            
            var sourceCloneUrl = message.MergeRequest.ObjectAttributes.Source.GitHttpUrl;
            var targetCloneUrl = message.MergeRequest.ObjectAttributes.Target.GitHttpUrl;

            using (var repo = await _repositoryCache.GetDirectoryForRepo(message.MergeRequest.ObjectAttributes.Target.Id.ToString()))
            {
                await FetchRemote(repo.Repository, $"{targetCloneUrl.GetHashCode():X}", targetCloneUrl);
                if (!targetCloneUrl.Equals(sourceCloneUrl, StringComparison.OrdinalIgnoreCase))
                    await FetchRemote(repo.Repository,  $"{sourceCloneUrl.GetHashCode():X}", sourceCloneUrl);

                var sourceCommit = repo.Repository.Lookup<Commit>(message.MergeRequest.ObjectAttributes.LastCommit.Id);
                var targetCommit = repo.Repository.Branches[$"{sourceCloneUrl.GetHashCode():X}/{message.MergeRequest.ObjectAttributes.TargetBranch}"].Tip;

                var users = await _changeset.GetUsersBetweenCommits(targetCommit, sourceCommit);

                var mergeRequest = await _api.GetMergeRequest(message.MergeRequest.Project.Id, message.MergeRequest.ObjectAttributes.Iid);
                var mergeRequestApprovals = await _api.GetMergeRequestApprovals(message.MergeRequest.Project.Id, message.MergeRequest.ObjectAttributes.Iid);

                var currentApprovers = mergeRequestApprovals.ApprovedBy.Select(x => x.User)
                    .Union(mergeRequestApprovals.SuggestedApprovers)
                    .ToList();

                var toAdd = new List<int>();
                var toRemove = new List<int>();
                
//                // Check for any reviewers to add
//                foreach (var user in users)
//                {
//                    if (currentApprovers.All(x => x.Username != user))
//                    {
//                        toAdd.Add(1);
//                        //toAdd.Add(user);
//                    }
//                }
//                // For check reviewers to remove
//                foreach (var currentApprover in currentApprovers)
//                {
//                    if (!users.Contains(currentApprover))
//                    {
//                        toRemove.Add(currentApprover);
//                    }
//                }
//
//                toAdd = toAdd.Distinct().ToList();
//                toRemove = toRemove.Distinct().ToList();
//
//                if (!toAdd.Any() && !toRemove.Any())
//                {
//                    // Pull request is update to date!
//                    return;
//                }

                var response = await _api.UpdateMergeRequestAllowApprovers(new UpdateApproversRequest
                {
                    ProjectId = message.MergeRequest.Project.Id,
                    MergeRequestIid = message.MergeRequest.ObjectAttributes.Iid,
                    ApproverIds = new List<int>{2,3}
                });

                Debug.WriteLine(response);
            }
        }

        private Task FetchRemote(IRepository repo, string remoteName, string remoteUrl)
        {
            return Task.Run(() =>
            {
                if (repo.Network.Remotes.All(x => x.Name != remoteName))
                    repo.Network.Remotes.Add(remoteName, remoteUrl);

                Commands.Fetch((LibGit2Sharp.Repository) repo, remoteName, new List<string>(), new FetchOptions(), "");

                return Task.CompletedTask;
            });
        }
    }
}