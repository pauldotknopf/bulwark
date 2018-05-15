using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bulwark.FileSystem;
using LibGit2Sharp;

namespace Bulwark.Strategy.CodeOwners.Impl
{
    public class CodeOwnersChangeset : ICodeOwnersChangeset
    {
        readonly ICodeOwnersWalker _walker;

        public CodeOwnersChangeset(ICodeOwnersWalker walker)
        {
            _walker = walker;
        }
        
        public async Task<List<string>> GetUsersForChangeset(Commit commit)
        {
            var repo = ((IBelongToARepository) commit).Repository;

            var parentCommits = commit.Parents.ToList();

            if (parentCommits.Count == 0)
            {
                throw new NotImplementedException("First commit not done yet");
            }else if (parentCommits.Count == 1)
            {
                var diff = repo.Diff.Compare<TreeChanges>(parentCommits.First().Tree, commit.Tree);
                var files = new List<string>();
                files.AddRange(diff.Added.Select(x => x.Path));
                files.AddRange(diff.Copied.Select(x => x.Path));
                files.AddRange(diff.Deleted.Select(x => x.Path));
                files.AddRange(diff.Modified.Select(x => x.Path));
                files.AddRange(diff.TypeChanged.Select(x => x.Path));
                files.AddRange(diff.Conflicted.Select(x => x.Path));
                files.AddRange(diff.Renamed.Select(x => x.Path));
                
                // We want to inspect the files on both the old commit and new commit.
                // This ensures we capture users who may have been removed from files
                // as a result of the commit.
                var oldUsers = await _walker.GetOwners(new RepositoryFileSystemProvider(parentCommits.First()), files.ToArray());
                var newUsers = await _walker.GetOwners(new RepositoryFileSystemProvider(commit), files.ToArray());
                return oldUsers.Union(newUsers).ToList();
            }
            else
            {
                throw new NotImplementedException("Merge not done yet");
            }
        }
    }
}