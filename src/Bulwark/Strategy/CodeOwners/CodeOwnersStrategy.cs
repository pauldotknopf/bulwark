using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bulwark.Strategy.CodeOwners
{
    public class CodeOwnersStrategy : IStrategy
    {
        public Task<List<string>> GetApprovers()
        {
            return Task.FromResult(new List<string>());
        }
    }
}