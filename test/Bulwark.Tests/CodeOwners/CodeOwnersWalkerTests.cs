using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bulwark.FileSystem;
using Bulwark.Strategy.CodeOwners;
using Bulwark.Strategy.CodeOwners.Impl;
using LibGit2Sharp;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace Bulwark.Tests.CodeOwners
{
    public class CodeOwnersWalkerTests : IDisposable
    {
        readonly WorkingDirectorySession _workingDirectory;
        readonly IFileProvider _fileProvider;
        readonly ICodeOwnersWalker _codeOwnersBuilder;

        public CodeOwnersWalkerTests()
        {
            _workingDirectory = new WorkingDirectorySession();
            _fileProvider = new PhysicalFileProvider(_workingDirectory.Directory);
            _codeOwnersBuilder = new CodeOwnersWalker(new CodeOwnersParser());
        }

        [Theory]
        [InlineData("*", "/testfile.txt")]
        [InlineData("*", "/test/testfile.txt")]
        [InlineData("**.txt", "/testfile.txt")]
        [InlineData("*.txt", "/testfile.txt")]
        public async Task Can_match_file(string pattern, string file)
        {
            Helpers.WriteCodeOwnersEntry(
                Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                pattern,
                "user1",
                "user2");
            var result = await _codeOwnersBuilder.GetOwners(_fileProvider, file);
            result.ShouldBeEqualTo(new List<string> {"user1", "user2"});
        }
        
        [Theory]
        [InlineData("!*", "/testfile.txt")]
        [InlineData("!*", "/test/testfile.txt")]
        [InlineData("!**.txt", "/testfile.txt")]
        [InlineData("!*.txt", "/testfile.txt")]
        public async Task Can_not_match_file(string pattern, string file)
        {
            Helpers.WriteCodeOwnersEntry(
                Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                pattern,
                "user1",
                "user2");
            var result = await _codeOwnersBuilder.GetOwners(_fileProvider, file);
            result.Count.ShouldBeEqualTo(0);
        }

        [Fact]
        public async Task Can_inherit_from_parent()
        {
            Helpers.WriteCodeOwners(
                Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                new CodeOwnerConfig().AddEntry("*", entry => entry.AddUser("user1")));
            Helpers.WriteCodeOwners(
                Path.Combine(_workingDirectory.Directory, "test1", "CODEOWNERS"),
                new CodeOwnerConfig()
                    .AddEntry("test2.pdf", entry => entry.AddUser("user2"))
                    .AddEntry("test2.txt", entry => entry.AddUser("user3")));
            
            var result = await _codeOwnersBuilder.GetOwners(_fileProvider, "/test1/test2.txt");
            
            Assert.Equal(new List<string>{ "user1", "user3" }, result);
        }

        [Fact]
        public async Task Can_inherit_from_parent_with_repo()
        {
            Helpers.WriteCodeOwners(
                Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                new CodeOwnerConfig().AddEntry("*", entry => entry.AddUser("user1")));
            Helpers.WriteCodeOwners(
                Path.Combine(_workingDirectory.Directory, "test1", "CODEOWNERS"),
                new CodeOwnerConfig()
                    .AddEntry("test2.pdf", entry => entry.AddUser("user2"))
                    .AddEntry("test2.txt", entry => entry.AddUser("user3")));

            Repository.Init(_workingDirectory.Directory);
            var author = new Signature("Paul Knopf", "pauldotknopf@gmail.com", DateTimeOffset.Now);
            using (var repo = new Repository(_workingDirectory.Directory))
            {
                Commands.Stage(repo, "*");
                var commit = repo.Commit("First commit", author, author);
                var provider = new RepositoryFileSystemProvider(commit);
                
                var result = await _codeOwnersBuilder.GetOwners(provider, "/test1/test2.txt");
            
                Assert.Equal(new List<string>{ "user1", "user3" }, result);
            }
        }
        
        [Fact]
        public async Task Can_remove_inherited_users()
        {
            Helpers.WriteCodeOwners(
                Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                new CodeOwnerConfig().AddEntry("*", entry => entry.AddUser("user1").AddUser("user2").AddUser("user3")));
            Helpers.WriteCodeOwners(
                Path.Combine(_workingDirectory.Directory, "test1", "CODEOWNERS"),
                new CodeOwnerConfig()
                    .AddEntry("test2.txt", entry => entry.AddUser("!user2")));
            
            var result = await _codeOwnersBuilder.GetOwners(_fileProvider, "/test1/test2.txt");
            
            Assert.Equal(new List<string>{ "user1", "user3" }, result);
        }

        public void Dispose()
        {
            _workingDirectory.Dispose();
        }
    }
}