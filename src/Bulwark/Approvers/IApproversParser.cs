using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bulwark.Approvers
{
    public interface IApproversParser
    {
        Task<ApproversConfig> ParseConfig(string content);
    }
}