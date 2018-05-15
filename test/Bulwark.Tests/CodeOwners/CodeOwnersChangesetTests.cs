﻿using System;
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
        
        public CodeOwnersChangesetTests()
        {
            _workingDirectory = new WorkingDirectorySession();
            _changeset = new CodeOwnersChangeset(new CodeOwnersWalker(new CodeOwnersParser()));
        }
        
        [Fact]
        public async Task Can_get_code_owners_based_on_new_file()
        {
            Repository.Init(_workingDirectory.Directory);
            Helpers.WriteCodeOwners(Path.Combine(_workingDirectory.Directory, "CODEOWNERS"),
                new CodeOwnerConfig()
                    .AddEntry("*", entry => entry.AddUser("user1"))
                    .AddEntry("*.txt", entry => entry.AddUser("user2")));
            var author = new Signature("Paul Knopf", "pauldotknopf@gmail.com", DateTimeOffset.Now);
            using (var repo = new Repository(_workingDirectory.Directory))
            {
                // Commit our code owners file.
                Commands.Stage(repo, "*");
                repo.Commit("First commit", author, author);
                
                // Add a new text file.
                File.WriteAllText(Path.Combine(_workingDirectory.Directory, "test.txt"), "");
                Commands.Stage(repo, "*");
                var commit = repo.Commit("Second commit", author, author);

                var users = await _changeset.GetUsersForChangeset(commit);
                
                // We should only have the users required for the new text file.
                Assert.Equal(users, new List<string> {"user1", "user2"});
                
                File.WriteAllText(Path.Combine(_workingDirectory.Directory, "test.md"), "");
                Commands.Stage(repo, "*");
                commit = repo.Commit("Second commit", author, author);

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