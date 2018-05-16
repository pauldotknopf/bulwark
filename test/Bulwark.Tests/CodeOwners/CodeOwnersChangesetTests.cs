using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bulwark.Strategy.CodeOwners;
using Bulwark.Strategy.CodeOwners.Impl;
using LibGit2Sharp;
using Xunit;

namespace Bulwark.Tests.CodeOwners
{
    public class CodeOwnersChangesetTests : IDisposable
    {
        readonly WorkingDirectorySession _workingDirectory;
        readonly ICodeOwnersChangeset _changeset;
        readonly Signature _author = new Signature("Paul Knopf", "pauldotknopf@gmail.com", DateTimeOffset.Now);
        
        public CodeOwnersChangesetTests()
        {
            _workingDirectory = new WorkingDirectorySession();
            _changeset = new CodeOwnersChangeset(new CodeOwnersWalker(new CodeOwnersParser()), new CodeOwnersParser());
        }

        [Fact]
        public async Task New_code_file_notifies_all_users()
        {
            Repository.Init(_workingDirectory.Directory);
            
            using (var repo = new Repository(_workingDirectory.Directory))
            {
                // Make the initial commit.
                Helpers.WriteCodeOwners(Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                    new CodeOwnerConfig()
                        .AddEntry("test1.txt", entry => entry.AddUser("user1")));
                Commands.Stage(repo, "*");
                var commit = repo.Commit("First commit", _author, _author);

                var users = await _changeset.GetUsersForChangeset(commit);
                
                Assert.Equal(users, new List<string> {"user1"});
                
                Directory.CreateDirectory(Path.Combine(_workingDirectory.Directory, "testdir"));
                Helpers.WriteCodeOwners(Path.Combine(_workingDirectory.Directory, "testdir", "CODEOWNERS"),
                    new CodeOwnerConfig()
                        .AddEntry("test2.txt", entry => entry.AddUser("user2")));
                Commands.Stage(repo, "*");
                commit = repo.Commit("Second commit", _author, _author);

                users = await _changeset.GetUsersForChangeset(commit);
                
                Assert.Equal(users, new List<string> {"user2"});
            }
        }
        
        [Fact]
        public async Task New_users_in_code_owners_file_gets_notified_regardless_of_path()
        {
            Repository.Init(_workingDirectory.Directory);
            
            using (var repo = new Repository(_workingDirectory.Directory))
            {
                // Make the initial commit.
                Helpers.WriteCodeOwners(Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                    new CodeOwnerConfig()
                        .AddEntry("test1.txt", entry => entry.AddUser("user1")));
                File.WriteAllText(Path.Combine(_workingDirectory.Directory, "test1.txt"), "test");
                Commands.Stage(repo, "*");
                repo.Commit("First commit", _author, _author);
                
                // Added a user to code file.
                // Only the new user should be notified, not the user currently in code file.
                Helpers.WriteCodeOwners(Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                    new CodeOwnerConfig()
                        .AddEntry("test1.txt", entry => entry.AddUser("user1"))
                        .AddEntry("test2.txt", entry => entry.AddUser("user2")));
                Commands.Stage(repo, "*");
                var commit = repo.Commit("Second commit", _author, _author);
                
                var users = await _changeset.GetUsersForChangeset(commit);
                
                Assert.Equal(users, new List<string> {"user2"});
            }
        }
        
        [Fact]
        public async Task New_commits_get_owners_for_entire_tree_since_no_diff()
        {
            Repository.Init(_workingDirectory.Directory);
            
            using (var repo = new Repository(_workingDirectory.Directory))
            {
                Helpers.WriteCodeOwners(Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                    new CodeOwnerConfig()
                        .AddEntry("*", entry => entry.AddUser("user1")));
                File.WriteAllText(Path.Combine(_workingDirectory.Directory, "test.txt"), "test");
                
                Commands.Stage(repo, "*");
                
                var users = await _changeset.GetUsersForChangeset(repo.Commit("First commit", _author, _author));
                
                // We the single "*" user.
                Assert.Equal(users, new List<string> {"user1"});
            }
        }

        [Fact]
        public async Task Can_get_users_based_on_merge()
        {
            Repository.Init(_workingDirectory.Directory);
            
            using (var repo = new Repository(_workingDirectory.Directory))
            {
                // First commit adds code file.
                Helpers.WriteCodeOwners(Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                    new CodeOwnerConfig()
                        .AddEntry("test1.txt", entry => entry.AddUser("user1"))
                        .AddEntry("test2.txt", entry => entry.AddUser("user2")));
                Commands.Stage(repo, "*");
                repo.Commit("First commit", _author, _author);
                
                // Create branch and add test1.txt
                repo.CreateBranch("test1");
                Commands.Checkout(repo, "test1");
                File.WriteAllText(Path.Combine(_workingDirectory.Directory, "test1.txt"), "test");
                Commands.Stage(repo, "*");
                var test1Commit = repo.Commit("Test 1 commit", _author, _author);
                
                // Create another branch and add test2.txt
                Commands.Checkout(repo, "master");
                repo.CreateBranch("test2");
                Commands.Checkout(repo, "test2");
                File.WriteAllText(Path.Combine(_workingDirectory.Directory, "test2.txt"), "test");
                Commands.Stage(repo, "*");
                repo.Commit("Test 2 commit", _author, _author);

                var mergeResult = repo.Merge(test1Commit, _author);

                var users = await _changeset.GetUsersForChangeset(mergeResult.Commit);
                Assert.Equal(users, new List<string>{ "user1", "user2" });
            }
        }
        
        [Fact]
        public async Task Can_get_code_owners_based_on_new_file()
        {
            Repository.Init(_workingDirectory.Directory);
            Helpers.WriteCodeOwners(Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                new CodeOwnerConfig()
                    .AddEntry("*", entry => entry.AddUser("user1"))
                    .AddEntry("*.txt", entry => entry.AddUser("user2")));
            
            using (var repo = new Repository(_workingDirectory.Directory))
            {
                // Commit our code owners file.
                Commands.Stage(repo, "*");
                repo.Commit("First commit", _author, _author);
                
                // Add a new text file.
                File.WriteAllText(Path.Combine(_workingDirectory.Directory, "test.txt"), "");
                Commands.Stage(repo, "*");
                var commit = repo.Commit("Second commit", _author, _author);

                var users = await _changeset.GetUsersForChangeset(commit);
                
                // We should only have the users required for the new text file.
                Assert.Equal(users, new List<string> {"user1", "user2"});
                
                File.WriteAllText(Path.Combine(_workingDirectory.Directory, "test.md"), "");
                Commands.Stage(repo, "*");
                commit = repo.Commit("Second commit", _author, _author);

                users = await _changeset.GetUsersForChangeset(commit);
                
                // We the single "*" user.
                Assert.Equal(users, new List<string> {"user1"});
            }
        }

        public void Dispose()
        {
            _workingDirectory.Dispose();
        }
    }
}