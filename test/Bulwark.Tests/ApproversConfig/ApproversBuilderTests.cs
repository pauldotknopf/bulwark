using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bulwark.Strategy.ApproversConfig;
using Bulwark.Strategy.ApproversConfig.Impl;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace Bulwark.Tests.ApproversConfig
{
    public class ApproversBuilderTests : IDisposable
    {
        readonly WorkingDirectorySession _workingDirectory;
        readonly IApproversBuilder _approversBuilder;

        public ApproversBuilderTests()
        {
            _workingDirectory = new WorkingDirectorySession();

            void EnsureParentDirectory(string path)
            {
                var directory = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(directory)) return;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            void WriteConfig(List<string> approvers, List<string> appendedApprovers, string path)
            {
                var sb = new StringBuilder();
                if(approvers != null)
                    sb.AppendLine($"Approvers: {string.Join(",", approvers)}");
                if(appendedApprovers != null)
                    sb.AppendLine($"AppendedApprovers: {string.Join(",", appendedApprovers)}");
                EnsureParentDirectory(path);
                File.WriteAllText(Path.Combine(_workingDirectory.Directory, path), sb.ToString());
            }
            void WriteFrontMatter(List<string> approvers, string path)
            {
                var sb = new StringBuilder();
                if (approvers != null)
                {
                    sb.AppendLine("---");
                    sb.AppendLine($"Approvers: {string.Join(",", approvers)}");
                    sb.AppendLine("---");
                }
                sb.AppendLine("Some other text...");
                EnsureParentDirectory(path);
                File.WriteAllText(Path.Combine(_workingDirectory.Directory, path), sb.ToString());
            }

            void WriteBinary(string path)
            {
                var bytes = new byte[2000];
                new Random().NextBytes(bytes);
                EnsureParentDirectory(path);
                File.WriteAllBytes(Path.Combine(_workingDirectory.Directory, path), bytes);
            }

            WriteConfig(new List<string> { "user1" }, new List<string> { "user1" }, ".approvers");
            WriteBinary("testbinary");
            WriteFrontMatter(new List<string>(), "file1.md");
            WriteFrontMatter(new List<string>(), "dir/file1.md");
            WriteFrontMatter(new List<string> { "user2" }, "dir/file2.md");
            WriteConfig(null, new List<string> { "user2" }, "dir/sub/.approvers");
            WriteBinary("dir/sub/testbinary");
            WriteFrontMatter(null, "dir/sub/file1.md");
            WriteFrontMatter(new List<string> { "user3" }, "dir/sub/file2.md");

            _approversBuilder = new ApproversBuilder(new ApproversParser());
        }

        [Fact(Skip = "Not implemented yet.")]
        public async Task Can_build_approvers_by_directory()
        {
            var result = await _approversBuilder.BuildApproversFromDirectory(new PhysicalFileProvider(_workingDirectory.Directory));

            Assert.Equal(result[".approvers"], new List<string> { "user1" });
            Assert.Equal(result["testbinary"], new List<string> { "user1" });
            Assert.Equal(result["file1.md"], new List<string> { "user1" });
            Assert.Equal(result["dir/file1.md"], new List<string> { "user1" });
            Assert.Equal(result["dir/file2.md"], new List<string> { "user2", "user1" });
            Assert.Equal(result["dir/sub/.approvers"], new List<string> { "user1", "user2" });
            Assert.Equal(result["dir/sub/file1.md"], new List<string> { "user1", "user2" });
            Assert.Equal(result["dir/sub/file2.md"], new List<string> { "user3", "user1", "user2" });
        }

        public void Dispose()
        {
            _workingDirectory.Dispose();
        }
    }
}