using System;
using System.Collections;
using System.Collections.Generic;
using LibGit2Sharp;
using Microsoft.Extensions.FileProviders;

namespace Bulwark.FileSystem
{
    public class RepoDirectoryContents : IDirectoryContents
    {
        readonly Tree _tree;
        List<IFileInfo> _matched;
        
        public RepoDirectoryContents(Tree tree)
        {
            _tree = tree;
        }

        public bool Exists => true;
        
        public IEnumerator<IFileInfo> GetEnumerator()
        {
            EnsureMatched();
            return _matched.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            EnsureMatched();
            return _matched.GetEnumerator();
        }
        
        private void EnsureMatched()
        {
            if (_matched == null)
            {
                var matched = new List<IFileInfo>();

                foreach (var entry in _tree)
                {
                    if (entry.TargetType == TreeEntryTargetType.Blob)
                    {
                        matched.Add(new RepoFileInfo(entry.Path, entry.Name, entry.Target as Blob));
                    }
                    else if (entry.TargetType == TreeEntryTargetType.Tree)
                    {
                        matched.Add(new RepoDirectoryInfo(entry.Path));
                    }
                    else
                    {
                        throw new NotImplementedException($"Haven't done {entry.TargetType} yet.");
                    }
                }
                
                _matched = matched;
            }
        }
    }
}