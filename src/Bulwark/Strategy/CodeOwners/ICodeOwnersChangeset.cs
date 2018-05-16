using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Bulwark.Strategy.CodeOwners
{
    public interface ICodeOwnersChangeset
    {
        Task<List<string>> GetUsersForChangeset(Commit commit);

        Task<List<string>> GetUsersBetweenCommits(Commit from, Commit to);
    }
}