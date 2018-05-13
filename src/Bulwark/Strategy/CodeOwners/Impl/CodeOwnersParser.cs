using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bulwark.Strategy.CodeOwners.Impl
{
    public class CodeOwnersParser : ICodeOwnersParser
    {
        public Task<CodeOwnerConfig> ParserConfig(string content)
        {
            var result = new CodeOwnerConfig();

            if (string.IsNullOrEmpty(content))
                return Task.FromResult(result);
            
            using (var reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    if (line.StartsWith("#")) continue;

                    var entries = line.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)
                        .SelectMany(x => x.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries))
                        .ToList();
                    var pattern = entries.First();
                    var users = entries.Skip(1)
                        .ToList();
                    
                    if(users.Count == 0)
                        throw new Exception($"You must provide a user for pattern {pattern}");
                    
                    result.Entries.Add(new CodeOwnerConfig.Entry
                    {
                        Pattern = pattern,
                        Users = users
                    });
                }
            }

            return Task.FromResult(result);
        }
    }
}