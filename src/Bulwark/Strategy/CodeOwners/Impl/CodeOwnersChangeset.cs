using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Bulwark.Strategy.CodeOwners.Impl
{
    public class CodeOwnersChangeset : ICodeOwnersChangeset
    {
        readonly ICodeOwnersWalker _walker;

        public CodeOwnersChangeset(ICodeOwnersWalker walker)
        {
            _walker = walker;
        }
        
        public async Task<List<string>> GetUsersForChangeset(Commit commit)
        {
            return new List<string>();
//            foreach (var change in commit.)
//            {
//                
//            }
        }
    }
}