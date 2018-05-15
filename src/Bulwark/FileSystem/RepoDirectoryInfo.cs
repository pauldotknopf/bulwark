using System;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace Bulwark.FileSystem
{
    public class RepoDirectoryInfo : IFileInfo
    {
        readonly string _path;

        public RepoDirectoryInfo(string path)
        {
            _path = path;
        }
        
        public Stream CreateReadStream()
        {
            throw new InvalidOperationException("Cannot create a stream for a directory.");
        }

        public bool Exists => true;
        public long Length => -1;
        public string PhysicalPath => $"/{_path}";
        public string Name => Path.GetFileName(_path);
        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public bool IsDirectory => true;
    }
}