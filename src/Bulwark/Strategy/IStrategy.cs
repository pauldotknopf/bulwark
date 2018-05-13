using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bulwark.Strategy
{
    public interface IStrategy
    {
        Task<List<string>> GetApprovers();
    }
}