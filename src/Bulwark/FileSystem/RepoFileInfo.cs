using System;
using System.IO;
using LibGit2Sharp;
using Microsoft.Extensions.FileProviders;

namespace Bulwark.FileSystem
{
    public class RepoFileInfo : IFileInfo
    {
        readonly string _path;
        readonly Blob _blob;

        public RepoFileInfo(string path, string name, Blob blob)
        {
            _path = path;
            Name = name;
            _blob = blob;
        }
        
        public Stream CreateReadStream()
        {
            return _blob.GetContentStream();
        }

        public bool Exists => true;
        public long Length => _blob.Size;
        public string PhysicalPath => $"/{_path}";
        public string Name { get; }
        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public bool IsDirectory => false;
    }
}