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
using Microsoft.Extensions.Logging;
using Commit = LibGit2Sharp.Commit;

namespace Bulwark.Integration.GitLab
{
    public class MergeRequestEventHandler : IMessageHandler<MergeRequestEvent>
    {
        readonly IRepositoryCache _repositoryCache;
        readonly ICodeOwnersChangeset _changeset;
        readonly IGitLabApi _api;
        readonly ILogger<MergeRequestEventHandler> _logger;

        public MergeRequestEventHandler(IRepositoryCache repositoryCache,
            ICodeOwnersChangeset changeset,
            IGitLabApi api,
            ILogger<MergeRequestEventHandler> logger)
        {
            _repositoryCache = repositoryCache;
            _changeset = changeset;
            _api = api;
            _logger = logger;
        }
        
        public async Task Handle(MergeRequestEvent message)
        {
            var mergeRequest = await _api.GetMergeRequest(
                message.MergeRequest.Project.Id,
                message.MergeRequest.ObjectAttributes.Iid);

            if (mergeRequest.WorkInProgress) return;

            if (mergeRequest.State == "closed") return;
            
            var sourceCloneUrl = message.MergeRequest.ObjectAttributes.Source.GitHttpUrl;
            var targetCloneUrl = message.MergeRequest.ObjectAttributes.Target.GitHttpUrl;

            using (var repo = await _repositoryCache.GetDirectoryForRepo(message.MergeRequest.ObjectAttributes.Target.Id.ToString()))
            {
                await FetchRemote(repo.Repository, $"{targetCloneUrl.GetHashCode():X}", targetCloneUrl);
                if (!targetCloneUrl.Equals(sourceCloneUrl, StringComparison.OrdinalIgnoreCase))
                    await FetchRemote(repo.Repository,  $"{sourceCloneUrl.GetHashCode():X}", sourceCloneUrl);

                var sourceCommit = repo.Repository.Lookup<Commit>(message.MergeRequest.ObjectAttributes.LastCommit.Id);
                var targetCommit = repo.Repository.Branches[$"{sourceCloneUrl.GetHashCode():X}/{message.MergeRequest.ObjectAttributes.TargetBranch}"].Tip;

                var codeOwnerUsers = await _changeset.GetUsersBetweenCommits(targetCommit, sourceCommit);

                var mergeRequestApprovals = await _api.GetMergeRequestApprovals(
                    message.MergeRequest.Project.Id,
                    message.MergeRequest.ObjectAttributes.Iid);

                var userIdLookup = new Dictionary<string, int>();

                var currentApprovers = mergeRequestApprovals.Approvers
                    .Select(x => x.User)
                    .ToList();
                
                // The author of the pull request will not be added as an approver.
                // It is assumed that they already approve of the changes.
                if (codeOwnerUsers.Contains(mergeRequest.Author.Username))
                    codeOwnerUsers.Remove(mergeRequest.Author.Username);
                
                foreach (var approver in currentApprovers)
                {
                    if(!userIdLookup.ContainsKey(approver.Username))
                        userIdLookup.Add(approver.Username, approver.Id);
                }

                bool usersChanged = false;
                foreach (var codeOwnerUser in codeOwnerUsers)
                {
                    if (currentApprovers.All(x => x.Username != codeOwnerUser))
                    {
                        usersChanged = true;
                    }
                }
                foreach (var currentApprover in currentApprovers)
                {
                    if (!codeOwnerUsers.Contains(currentApprover.Username))
                    {
                        usersChanged = true;
                    }
                }

                if (!usersChanged)
                {
                    // No users may have changed, but the approvers request count may be off, ensure it is up to date.
                    // There may be no changes, but the required approvers count may be off.
                    if (mergeRequestApprovals.ApprovalsRequired != mergeRequestApprovals.Approvers.Count)
                    {
                        await _api.UpdateMergeRequestApprovals(new ChangeApprovalConfigurationRequest
                        {
                            ProjectId = message.MergeRequest.Project.Id,
                            MergeRequestIid = message.MergeRequest.ObjectAttributes.Iid,
                            ApprovalsRequired = mergeRequestApprovals.Approvers.Count
                        });
                    }
                }
                else
                {
                    // Users need to be updated!
                    // We need to collect the user ids of all the users to add to the pull request.
                    foreach (var codeOwnerUser in codeOwnerUsers)
                    {
                        if (!userIdLookup.ContainsKey(codeOwnerUser))
                        {
                            var users = await _api.GetUsers(new UsersRequest
                            {
                                Username = codeOwnerUser
                            });

                            if (users.Count == 0)
                            {
                                _logger.LogError("No user found with {Username}.", codeOwnerUser);
                                return;
                            }

                            if (users.Count > 1)
                            {
                                _logger.LogError("Multiple users found with {Username}.", codeOwnerUser);
                                return;
                            }

                            userIdLookup.Add(users.First().Username, users.First().Id);
                        }
                    }

                    mergeRequestApprovals = await _api.UpdateMergeRequestAllowApprovers(new UpdateApproversRequest
                    {
                        ProjectId = message.MergeRequest.Project.Id,
                        MergeRequestIid = message.MergeRequest.ObjectAttributes.Iid,
                        ApproverIds = codeOwnerUsers.Select(x => userIdLookup[x]).ToList()
                    });
                    
                    // Let's see if we need to update the approver count.
                    if (mergeRequestApprovals.ApprovalsRequired != mergeRequestApprovals.Approvers.Count)
                    {
                        await _api.UpdateMergeRequestApprovals(new ChangeApprovalConfigurationRequest
                        {
                            ProjectId = message.MergeRequest.Project.Id,
                            MergeRequestIid = message.MergeRequest.ObjectAttributes.Iid,
                            ApprovalsRequired = mergeRequestApprovals.Approvers.Count
                        });
                    }
                }
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