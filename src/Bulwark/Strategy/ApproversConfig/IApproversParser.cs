using System.Threading.Tasks;

namespace Bulwark.Strategy.ApproversConfig
{
    public interface IApproversParser
    {
        Task<ApproversConfig> ParseConfig(string content);
    }
}