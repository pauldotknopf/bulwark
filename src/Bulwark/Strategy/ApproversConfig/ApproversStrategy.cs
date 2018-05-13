using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bulwark.Strategy.ApproversConfig
{
    public class ApproversConfigStrategy : IStrategy
    {
        public Task<List<string>> GetApprovers()
        {
            return Task.FromResult(new List<string>());
        }
    }
}