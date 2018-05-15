using System;
using System.IO;
using Bulwark.Strategy.CodeOwners;
using LibGit2Sharp;
using Xunit;

namespace Bulwark.Tests.CodeOwners
{
    public class CodeOwnersDifferTests : IDisposable
    {
        readonly WorkingDirectorySession _workingDirectory;
        
        public CodeOwnersDifferTests()
        {
            _workingDirectory = new WorkingDirectorySession();
        }
        
        [Fact]
        public void Can_get_code_owners_based_on_changeset()
        {
            Repository.Init(_workingDirectory.Directory);
            Helpers.WriteCodeOwners(Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                new CodeOwnerConfig()
                    .AddEntry("test1.txt", entry => entry.AddUser("user1").AddUser("user2"))
                    .AddEntry("test2.txt", entry => entry.AddUser("user3").AddUser("user4")));
            var author = new Signature("Paul Knopf", "pauldotknopf@gmail.com", DateTimeOffset.Now);
            using (var repo = new Repository(_workingDirectory.Directory))
            {
                repo.Index.Add(".");
                var commit = repo.Commit("First commit", author, author);
            }
        }

        public void Dispose()
        {
            _workingDirectory.Dispose();
        }
    }
}