using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Bulwark.Strategy.CodeOwners
{
    public interface ICodeOwnersWalker
    {
        Task<List<string>> GetOwners(IFileProvider provider, params string[] paths);
    }
}