using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bulwark.Strategy.ApproversConfig.Impl;
using Bulwark.Strategy.CodeOwners;
using Bulwark.Strategy.CodeOwners.Impl;
using FluentAssert;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Xunit;

namespace Bulwark.Tests.CodeOwners
{
    public class CodeOwnersBuilderTests : IDisposable
    {
        readonly WorkingDirectorySession _workingDirectory;
        readonly IFileProvider _fileProvider;
        readonly ICodeOwnersBuilder _codeOwnersBuilder;

        public CodeOwnersBuilderTests()
        {
            _workingDirectory = new WorkingDirectorySession();
            _fileProvider = new PhysicalFileProvider(_workingDirectory.Directory);
            _codeOwnersBuilder = new CodeOwnersBuilder(new CodeOwnersParser());
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
                    .AddEntry("test2.pdf", entry => entry.AddUser("user2")));
            Helpers.WriteCodeOwners(
                Path.Combine(_workingDirectory.Directory, "test1", "CODEOWNERS"),
                new CodeOwnerConfig()
                    .AddEntry("test2.txt", entry => entry.AddUser("user3")));
            
            var result = await _codeOwnersBuilder.GetOwners(_fileProvider, "/test1/test2.txt");
            
            Assert.Equal(new List<string>{ "user1", "user3" }, result);
        }
        
        public void Dispose()
        {
            _workingDirectory.Dispose();
        }
    }
}