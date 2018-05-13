using System.Threading.Tasks;

namespace Bulwark.Strategy.CodeOwners
{
    public interface ICodeOwnersParser
    {
        Task<CodeOwnerConfig> ParserConfig(string content);
    }
}