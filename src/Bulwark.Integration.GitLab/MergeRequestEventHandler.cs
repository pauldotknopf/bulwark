using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bulwark.Integration.Messages;
using Bulwark.Integration.Repository;
using Bulwark.Strategy.CodeOwners;
using LibGit2Sharp;

namespace Bulwark.Integration.GitLab
{
    public class MergeRequestEventHandler : IMessageHandler<MergeRequestEvent>
    {
        readonly IRepositoryCache _repositoryCache;
        readonly ICodeOwnersChangeset _changeset;

        public MergeRequestEventHandler(IRepositoryCache repositoryCache,
            ICodeOwnersChangeset changeset)
        {
            _repositoryCache = repositoryCache;
            _changeset = changeset;
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