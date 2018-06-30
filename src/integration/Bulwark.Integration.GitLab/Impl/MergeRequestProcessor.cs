using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bulwark.Integration.GitLab.Api;
using Bulwark.Integration.GitLab.Api.Requests;
using Bulwark.Integration.GitLab.Api.Types;
using Bulwark.Integration.Repository;
using Bulwark.Strategy.CodeOwners;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bulwark.Integration.GitLab.Impl
{
    public class MergeRequestProcessor : IMergeRequestProcessor
    {
        readonly ILogger<MergeRequestProcessor> _logger;
        readonly GitLabOptions _options;
        readonly IGitLabApi _api;
        readonly IRepositoryCache _repositoryCache;
        readonly ICodeOwnersChangeset _changeset;

        public MergeRequestProcessor(IGitLabApi api,
            IRepositoryCache repositoryCache,
            ICodeOwnersChangeset changeset,
            ILogger<MergeRequestProcessor> logger,
            IOptions<GitLabOptions> options)
        {
            _api = api;
            _repositoryCache = repositoryCache;
            _changeset = changeset;
            _logger = logger;
            _options = options.Value;
        }
        
        public async Task ProcessMergeRequest(int projectId, int mergeRequestIid)
        {
            var mergeRequest = await _api.GetMergeRequest(projectId, mergeRequestIid);

            if (mergeRequest.WorkInProgress)
            {
                _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: Skipping because work in progress.", mergeRequest.Iid, mergeRequest.ProjectId);
                return;
            }

            if (mergeRequest.State != MergeRequestState.Opened)
            {
                _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: Skipping because merge request not opened.", mergeRequest.Iid, mergeRequest.ProjectId);
                return;
            }

            // Let's see if this is a merge request that we should listen too.
            if (!string.IsNullOrEmpty(_options.TargetBranchesFilter))
            {
                if (!Regex.IsMatch(mergeRequest.TargetBranch, _options.TargetBranchesFilter))
                {
                    _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: Skipping {TargetBranch} due to filter.", mergeRequest.Iid, mergeRequest.ProjectId, mergeRequest.TargetBranch);
                    return;
                }
            }
            
            var targetProject = await _api.GetProject(new ProjectRequest {ProjectId = mergeRequest.TargetProjectId});
            var sourceProject = targetProject.Id == mergeRequest.SourceProjectId
                ? targetProject
                : await _api.GetProject(new ProjectRequest {ProjectId = mergeRequest.SourceProjectId});
            
            var sourceCloneUrl = sourceProject.SshUrlToRepo;
            var targetCloneUrl = targetProject.SshUrlToRepo;
            UsernamePasswordCredentials credentials = null;
                
            if (_options.UseHttp)
            {
                sourceCloneUrl = sourceProject.HttpUrlToRepo;
                targetCloneUrl = targetProject.HttpUrlToRepo;
                if (!string.IsNullOrEmpty(_options.HttpUsername) && !string.IsNullOrEmpty(_options.HttpPassword))
                {
                    credentials = new UsernamePasswordCredentials
                    {
                        Username = _options.HttpUsername,
                        Password = _options.HttpPassword
                    };
                }
            }
            
            using (var repo = await _repositoryCache.GetDirectoryForRepo(mergeRequest.Id.ToString()))
            {
                _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: Fething repo to {Location}.", mergeRequest.Iid, mergeRequest.ProjectId, repo.Location);

                await FetchRemote(repo.Repository, $"{targetCloneUrl.GetHashCode():X}", targetCloneUrl, credentials);
                if (!targetCloneUrl.Equals(sourceCloneUrl, StringComparison.OrdinalIgnoreCase))
                    await FetchRemote(repo.Repository,  $"{sourceCloneUrl.GetHashCode():X}", sourceCloneUrl, credentials);

                _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: Checking out commits.", mergeRequest.Iid, mergeRequest.ProjectId);

                var sourceCommit = repo.Repository.Lookup<LibGit2Sharp.Commit>(mergeRequest.Sha);
                var targetCommit = repo.Repository.Branches[$"{targetCloneUrl.GetHashCode():X}/{mergeRequest.TargetBranch}"].Tip;

                _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: Getting a list of code owners.", mergeRequest.Iid, mergeRequest.ProjectId);
                
                var codeOwnerUsers = await _changeset.GetUsersBetweenCommits(targetCommit, sourceCommit);

                _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: Found {Users}.", mergeRequest.Iid, mergeRequest.ProjectId, codeOwnerUsers);
                
                _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: Getting the current approvers.", mergeRequest.Iid, mergeRequest.ProjectId);
                
                var mergeRequestApprovals = await _api.GetMergeRequestApprovals(mergeRequest.ProjectId, mergeRequest.Iid);

                var userIdLookup = new Dictionary<string, int>();

                var currentApprovers = mergeRequestApprovals.Approvers
                    .Select(x => x.User)
                    .ToList();
                
                _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: {Users} are currently assigned to the merge request.", mergeRequest.Iid, mergeRequest.ProjectId, currentApprovers.Select(x => x.Username));
                
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
                    _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: No changes in approvers.", mergeRequest.Iid, mergeRequest.ProjectId);
                    
                    // No users may have changed, but the approvers request count may be off, ensure it is up to date.
                    // There may be no changes, but the required approvers count may be off.
                    if (mergeRequestApprovals.ApprovalsRequired != mergeRequestApprovals.Approvers.Count)
                    {
                        _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: The number of approvers required on mr is off, updating.", mergeRequest.Iid, mergeRequest.ProjectId);
                        mergeRequestApprovals = await _api.UpdateMergeRequestApprovals(new ChangeApprovalConfigurationRequest
                        {
                            ProjectId = mergeRequest.ProjectId,
                            MergeRequestIid = mergeRequest.Iid,
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

                    _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: Updating approvers with {UserIdLookup}.", mergeRequest.Iid, mergeRequest.ProjectId, codeOwnerUsers);
                    
                    mergeRequestApprovals = await _api.UpdateMergeRequestAllowApprovers(new UpdateApproversRequest
                    {
                        ProjectId = mergeRequest.ProjectId,
                        MergeRequestIid = mergeRequest.Iid,
                        ApproverIds = codeOwnerUsers.Select(x => userIdLookup[x]).ToList()
                    });
                    
                    // Let's see if we need to update the approver count.
                    if (mergeRequestApprovals.ApprovalsRequired != mergeRequestApprovals.Approvers.Count)
                    {
                        _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: The number of approvers required on mr is off, updating.", mergeRequest.Iid, mergeRequest.ProjectId);
                        mergeRequestApprovals = await _api.UpdateMergeRequestApprovals(new ChangeApprovalConfigurationRequest
                        {
                            ProjectId = mergeRequest.ProjectId,
                            MergeRequestIid = mergeRequest.Iid,
                            ApprovalsRequired = mergeRequestApprovals.Approvers.Count
                        });
                    }
                }

                if (_options.AutoMergePullRequests && mergeRequestApprovals.ApprovalsLeft == 0)
                {
                    _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: Checking to see if all the required users approved before we auto merge.", mergeRequest.Iid, mergeRequest.ProjectId);
                    
                    // We have no more approvals, let's merge this merge request.
                    // But first, let's make sure that the approvers are on the required list.
                    bool allApproved = true;
                    foreach (var user in codeOwnerUsers)
                    {
                        if (!mergeRequestApprovals.ApprovedBy.Any(x => x.User != null && x.User.Username == user))
                        {
                            allApproved = false;
                        }
                    }

                    if (allApproved)
                    {
                        _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: All approved, merging the request.", mergeRequest.Iid, mergeRequest.ProjectId);

                        // This MR can be merged!
                        await _api.AcceptMergeRequest(new AcceptMergeRequestRequest
                        {
                            ProjectId = mergeRequest.ProjectId,
                            MergeRequestIid = mergeRequest.Iid,
                            Sha = mergeRequest.Sha, // This is to ensure we are merging what we expect.
                            // Passing null when empty ensures field won't get sent to GitLab, ensuring
                            // it will use the default commit message, instead of an empty one.
                            MergeCommitMessage = string.IsNullOrEmpty(_options.MergeCommitMessage) 
                                ? null 
                                : _options.MergeCommitMessage,
                            MergeWhenPipelineSuceeds = _options.MergeWhenPipelineSuceeds,
                            ShouldRemoveSourceBranch = _options.ShouldRemoveSourceBranch
                        });
                    }
                }
                
                _logger.LogDebug("MR:{MergeRequestIid}:Project:{ProjectId}: Finished with {ApprovalsLeft} approvals left.", mergeRequest.Iid, mergeRequest.ProjectId, mergeRequestApprovals.ApprovalsLeft);
            }
        }
        
        private Task FetchRemote(IRepository repo, string remoteName, string remoteUrl, UsernamePasswordCredentials credentials)
        {
            _logger.LogDebug("Adding {RemoteName} ({RemoteUrl}) to repo.", remoteName, remoteUrl);
            
            return Task.Run(() =>
            {
                if (repo.Network.Remotes.All(x => x.Name != remoteName))
                {
                    _logger.LogDebug("Adding {RemoteName} to repo for first time.", remoteName);
                    repo.Network.Remotes.Add(remoteName, remoteUrl);
                }
                else
                {
                    _logger.LogDebug("Adding {RemoteName} already exists in repo.", remoteName);
                }

                var fetchOptions = new FetchOptions();
                if (credentials != null)
                {
                    _logger.LogDebug("Using credentials for {RemoteName}", remoteName);
                    fetchOptions.CredentialsProvider = (url, usernameFromUrl, types) => credentials;
                }
                else
                {
                    _logger.LogDebug("Using anonymous access");
                }
                
                _logger.LogDebug("Fetching from {RemoteName}...", remoteName);
                
                Commands.Fetch((LibGit2Sharp.Repository) repo, remoteName, new List<string>(), fetchOptions, "");

                _logger.LogDebug("Successfuly fetched from {RemoteName}.", remoteName);
                
                return Task.CompletedTask;
            });
        }
    }
}