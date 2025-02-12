using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

                    var splitIndex = line.IndexOf(" ", StringComparison.InvariantCulture);
                    
                    var pattern = line.Substring(0, splitIndex);
                    var usersString = line.Substring(splitIndex + 1);
                    
                    var match = Regex.Matches(usersString, @"([\""""].+?[\""""])|[^ ,]+");

                    var users = match.Cast<Match>().Select(x => x.Value.TrimStart('\"').TrimStart('@').TrimEnd('\"')).ToList();
                    
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