using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Bulwark.Strategy.ApproversConfig
{
    public interface IApproversBuilder
    {
        Task<Dictionary<string, List<string>>> BuildApproversFromDirectory(IFileProvider fileProvider);
    }
}