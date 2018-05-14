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
        readonly ICodeOwnersBuilder _codeOwnersBuilder;

        public CodeOwnersBuilderTests()
        {
            _workingDirectory = new WorkingDirectorySession();
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
            var result = await _codeOwnersBuilder.GetOwners(new PhysicalFileProvider(_workingDirectory.Directory), file);
            result.ShouldBeEqualTo(new List<string> {"user1", "user2"});
        }

        public void Dispose()
        {
            _workingDirectory.Dispose();
        }
    }
}