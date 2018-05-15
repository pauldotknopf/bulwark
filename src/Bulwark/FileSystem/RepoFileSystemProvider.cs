using System;
using LibGit2Sharp;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Bulwark.FileSystem
{
    public class RepositoryFileSystemProvider : IFileProvider
    {
        readonly Commit _commit;

        public RepositoryFileSystemProvider(Commit commit)
        {
            _commit = commit;
        }
        
        public IFileInfo GetFileInfo(string subpath)
        {
            var parts = subpath.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            var current = _commit.Tree;
            for (var x = 0; x < parts.Length; x++)
            {
                var part = parts[x];
                if (x == parts.Length - 1)
                {
                    // This is the last part of the path, it should be a file.
                    // ReSharper disable PossibleNullReferenceException
                    foreach (var entry in current)
                    // ReSharper restore PossibleNullReferenceException
                    {
                        if(entry.TargetType != TreeEntryTargetType.Blob) continue;
                        if(entry.Name != part) continue;
                        return new RepoFileInfo(entry.Path, entry.Name, entry.Target as Blob);
                    }
                }
                else
                {
                    // ReSharper disable PossibleNullReferenceException
                    foreach (var entry in current)
                    // ReSharper restore PossibleNullReferenceException
                    {
                        if(entry.TargetType != TreeEntryTargetType.Tree) continue;
                        if(entry.Name != part) continue;
                        current = entry.Target as Tree;
                        break;
                    }
                }
            }
            return new NotFoundFileInfo(subpath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var parts = subpath.Split(new [] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            var current = _commit.Tree;
            foreach (var part in parts)
            {
                var found = false;
                // ReSharper disable PossibleNullReferenceException
                foreach (var entry in current)
                // ReSharper restore PossibleNullReferenceException
                {
                    if(found) continue;
                    if (entry.TargetType != TreeEntryTargetType.Tree) continue;
                    if (entry.Name != part) continue;
                    current = entry.Target as Tree;
                    found = true;
                }
                if(!found)
                    return new NotFoundDirectoryContents();
            }
            
            return new RepoDirectoryContents(current);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }
    }
}