using System;
using System.IO;
using Bulwark.FileSystem;
using LibGit2Sharp;
using Xunit;

namespace Bulwark.Tests
{
    public class RepositoryFileProviderTests : IDisposable
    {
        readonly WorkingDirectorySession _workingDirectory;
        readonly Repository _repository;
        
        public RepositoryFileProviderTests()
        {
            _workingDirectory = new WorkingDirectorySession();
            Repository.Init(_workingDirectory.Directory);
            _repository = new Repository(_workingDirectory.Directory);
        }
        
        [Fact]
        public void Can_view_repository()
        {
            File.WriteAllText(Path.Combine(_workingDirectory.Directory, "test1.txt"), "test");
            Directory.CreateDirectory(Path.Combine(_workingDirectory.Directory, "test2"));
            File.WriteAllText(Path.Combine(_workingDirectory.Directory, "test2", "test3.txt"), "test");
            Directory.CreateDirectory(Path.Combine(_workingDirectory.Directory, "test2", "test4"));
            File.WriteAllText(Path.Combine(_workingDirectory.Directory, "test2", "test4", "test5.txt"), "test");

            var commit = Commit();

            var provider = new RepositoryFileSystemProvider(commit);

            foreach (var contents in provider.GetDirectoryContents("/"))
            {
                switch (contents.Name)
                {
                    case "test1.txt":
                        contents.Exists.ShouldBeTrue();
                        contents.IsDirectory.ShouldBeFalse();
                        contents.PhysicalPath.ShouldBeEqualTo("/test1.txt");
                        break;
                    case "test2":
                        contents.Exists.ShouldBeEqualTo(true);
                        contents.IsDirectory.ShouldBeTrue();
                        //contents.PhysicalPath.ShouldBeEqualTo("/test2");
                        break;
                    default:
                        Assert.True(false, $"Unexpected name {contents.Name}");
                        break;
                }
            }

            foreach (var contents in provider.GetDirectoryContents("/test2"))
            {
                switch (contents.Name)
                {
                    case "test3.txt":
                        contents.Exists.ShouldBeTrue();
                        contents.IsDirectory.ShouldBeFalse();
                        break;
                    case "test4":
                        contents.Exists.ShouldBeTrue();
                        contents.IsDirectory.ShouldBeTrue();
                        break;
                    default:
                        Assert.True(false, $"Unexpected name {contents.Name}");
                        break;
                }
            }
            
            foreach (var contents in provider.GetDirectoryContents("/test2/test4"))
            {
                switch (contents.Name)
                {
                    case "test5.txt":
                        contents.Exists.ShouldBeTrue();
                        contents.IsDirectory.ShouldBeFalse();
                        break;
                    default:
                        Assert.True(false, $"Unexpected name {contents.Name}");
                        break;
                }
            }

            var info = provider.GetFileInfo("/test1.txt");
            info.Exists.ShouldBeTrue();
            info.IsDirectory.ShouldBeFalse();
            info.Name.ShouldBeEqualTo("test1.txt");
            
            info = provider.GetFileInfo("/test2");
            info.Exists.ShouldBeFalse();
            
            info = provider.GetFileInfo("/test2/test3.txt");
            info.Exists.ShouldBeTrue();
            info.IsDirectory.ShouldBeFalse();
            info.Name.ShouldBeEqualTo("test3.txt");
            
            info = provider.GetFileInfo("/test/test4");
            info.Exists.ShouldBeFalse();
            
            info = provider.GetFileInfo("/test2/test4/test5.txt");
            info.Exists.ShouldBeTrue();
            info.IsDirectory.ShouldBeFalse();
            info.Name.ShouldBeEqualTo("test5.txt");
        }
        
        private Commit Commit()
        {
            Commands.Stage(_repository, "*");
            var author = new Signature("Paul Knopf", "pauldotknopf@gmail.com", DateTimeOffset.Now);
            return _repository.Commit("First commit", author, author);
        }
        
        public void Dispose()
        {
            _repository.Dispose();
            _workingDirectory.Dispose();
        }
    }
}