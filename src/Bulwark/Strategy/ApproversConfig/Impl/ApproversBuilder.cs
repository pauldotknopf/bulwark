using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Bulwark.Strategy.ApproversConfig.Impl
{
    public class ApproversBuilder : IApproversBuilder
    {
        private readonly IApproversParser _approversParser;

        public ApproversBuilder(IApproversParser approversParser)
        {
            _approversParser = approversParser;
        }
        
        public Task<Dictionary<string, List<string>>> BuildApproversFromDirectory(IFileProvider fileProvider)
        {
            return Task.FromResult(new Dictionary<string, List<string>>());
        }
    }
}