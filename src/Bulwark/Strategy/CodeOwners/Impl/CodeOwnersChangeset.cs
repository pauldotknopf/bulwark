using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Bulwark.FileSystem;
using LibGit2Sharp;

namespace Bulwark.Strategy.CodeOwners.Impl
{
    public class CodeOwnersChangeset : ICodeOwnersChangeset
    {
        readonly ICodeOwnersWalker _walker;
        readonly ICodeOwnersParser _parser;

        public CodeOwnersChangeset(ICodeOwnersWalker walker, ICodeOwnersParser parser)
        {
            _walker = walker;
            _parser = parser;
        }
        
        public async Task<List<string>> GetUsersForChangeset(Commit commit)
        {
            var repo = ((IBelongToARepository) commit).Repository;

            var parentCommits = commit.Parents.ToList();
            
            if (parentCommits.Count == 0)
            {
                // Since this is the first commit, let's just find all the code config files
                // and return all the users found.
                var codeConfigs = new List<CodeOwnerConfig>();
                
                async Task WalkTree(Tree tree)
                {
                    foreach (var entry in tree)
                    {
                        if (entry.TargetType == TreeEntryTargetType.Tree)
                        {
                            await WalkTree(entry.Target as Tree);
                        }
                        else if(entry.TargetType == TreeEntryTargetType.Blob)
                        {
                            if (entry.Name == "CODEOWNERS")
                            {
                                using (var stream = ((Blob) entry.Target).GetContentStream())
                                {
                                    using (var reader = new StreamReader(stream))
                                    {
                                        codeConfigs.Add(await _parser.ParserConfig(reader.ReadToEnd()));
                                    }
                                }
                            }
                        }
                        else
                        {
                            // TODO: What is this?
                            throw new NotImplementedException();
                        }
                    }
                }
                
                await WalkTree(commit.Tree);

                return codeConfigs.SelectMany(x => x.Entries)
                    .SelectMany(x => x.Users)
                    .Select(x => x.StartsWith("!") ? x.Substring(1) : x)
                    .Distinct()
                    .ToList();
            }

            var users = new HashSet<string>();

            foreach (var parentCommit in parentCommits)
            {
                foreach (var user in await GetUsersBetweenCommits(parentCommit, commit))
                {
                    if (!users.Contains(user))
                        users.Add(user);
                }
            }

            return users.ToList();
        }

        public async Task<List<string>> GetUsersBetweenCommits(Commit from, Commit to)
        {   
            var repo = ((IBelongToARepository) from).Repository;

            var users = new HashSet<string>();

            var changes = repo.Diff.Compare<TreeChanges>(from.Tree, to.Tree);
            var files = new List<string>();
            files.AddRange(changes.Added.Select(x => x.Path));
            files.AddRange(changes.Copied.Select(x => x.Path));
            files.AddRange(changes.Deleted.Select(x => x.Path));
            files.AddRange(changes.Modified.Select(x => x.Path));
            files.AddRange(changes.TypeChanged.Select(x => x.Path));
            files.AddRange(changes.Conflicted.Select(x => x.Path));
            files.AddRange(changes.Renamed.Select(x => x.Path));
            
            // Since we have a list of all the files changed between these two commits,
            // We want to inspect the files on both the old commit and new commit.
            // This ensures we capture users who may have been removed from files
            // as a result of the commit.
            var oldUsers = await _walker.GetOwners(new RepositoryFileSystemProvider(from), files.ToArray());
            var newUsers = await _walker.GetOwners(new RepositoryFileSystemProvider(to), files.ToArray());
            
            foreach (var oldUser in oldUsers)
            {
                if (!users.Contains(oldUser))
                    users.Add(oldUser);
            }
            foreach (var newUser in newUsers)
            {
                if (!users.Contains(newUser))
                    users.Add(newUser);
            }

            async Task CheckForCodeOwnerUsers(TreeEntryChanges entryChanges)
            {
                if (Path.GetFileName(entryChanges.Path) == "CODEOWNERS")
                {
                    var diff = repo.Diff.Compare(repo.Lookup<Blob>(entryChanges.OldOid),
                        repo.Lookup<Blob>(entryChanges.Oid), new CompareOptions
                        {
                            ContextLines = 0,
                            IncludeUnmodified = false
                        });
                    if (!diff.IsBinaryComparison)
                    {
                        var unifiedConfig = await GetCodeOwnerConfigFromChanges(diff);
                        foreach (var user in unifiedConfig.Entries.SelectMany(x => x.Users))
                        {
                            var realUser = user.StartsWith("!") ? user.Substring(1) : user;
                            if (!users.Contains(realUser))
                                users.Add(realUser);
                        }
                    }
                }
            }
            
            // In addition to matching users based on path alone, let's look to see if any
            // CODEOWNERS file was changed, and if so, call out the users that were added/removed.
            foreach (var added in changes.Added)
            {
                await CheckForCodeOwnerUsers(added);
            }
            foreach (var copied in changes.Copied)
            {
                await CheckForCodeOwnerUsers(copied);
            }
            foreach (var deleted in changes.Deleted)
            {
                await CheckForCodeOwnerUsers(deleted);
            }
            foreach (var modified in changes.Modified)
            {
                await CheckForCodeOwnerUsers(modified);
            }
            foreach (var typeChanged in changes.TypeChanged)
            {
                await CheckForCodeOwnerUsers(typeChanged);
            }
            foreach (var renamed in changes.Renamed)
            {
                await CheckForCodeOwnerUsers(renamed);
            }

            return users.ToList();
        }
        
        public Task<CodeOwnerConfig> GetCodeOwnerConfigFromChanges(ContentChanges changes)
        {
            var result = new StringBuilder();
            using (var reader = new StringReader(changes.Patch))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if(line.StartsWith("@")) continue;
                    
                    if (line.StartsWith("+"))
                    {
                        result.AppendLine(line.Substring(1));
                    }
                    else if (line.StartsWith("-"))
                    {
                        result.AppendLine(line.Substring(1));
                    }
                }
            }

            return _parser.ParserConfig(result.ToString());
        }
    }
}