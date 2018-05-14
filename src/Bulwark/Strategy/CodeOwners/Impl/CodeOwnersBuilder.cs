using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using SharpWildmatch;

namespace Bulwark.Strategy.CodeOwners.Impl
{
    public class CodeOwnersBuilder : ICodeOwnersBuilder
    {
        readonly ICodeOwnersParser _parser;

        public CodeOwnersBuilder(ICodeOwnersParser parser)
        {
            _parser = parser;
        }
        
        public async Task<List<string>> GetOwners(IFileProvider provider, string path)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException();
            if(!path.StartsWith("/")) throw new Exception("All paths must start with a /");

            await WalkParentsForCodeOwners(provider, path, (directory, relativePath, config) =>
            {
                foreach (var entry in config.Entries)
                {
                    if(Wildmatch.Match(entry.Pattern.TrimStart('/'), relativePath.TrimStart('/'), MatchFlags.CaseFold) == MatchResult.Match)
                    {
                        foreach (var user in entry.Users)
                        {
                            if (user.StartsWith("!"))
                            {
                                if (result.Contains(user.Substring(1)))
                                    result.Remove(user.Substring(1));
                            }
                            else
                            {
                                if(!result.Contains(user))
                                    result.Add(user);
                            }
                        }
                    }
                }

                return Task.CompletedTask;
            });
            
            return result;
        }

        private async Task WalkParentsForCodeOwners(IFileProvider provider, string path, Func<string, string, CodeOwnerConfig, Task> callback)
        {
            var results = new Dictionary<Tuple<string, string>, CodeOwnerConfig>();
    
            async Task WalkParent(string directory)
            {
                var parent = Path.GetDirectoryName(directory);
                
                if (string.IsNullOrEmpty(parent)) return; // this is the root

                var config = await GetConfigForDirectory(provider, parent);
                
                if (config != null)
                {
                    results.Add(new Tuple<string, string>(parent, path.Substring(parent.Length)), config);   
                }

                await WalkParent(parent);
            }
            
            await WalkParent(path);

            foreach (var result in results.Reverse())
            {
                await callback(result.Key.Item1, result.Key.Item2, result.Value);
            }
        }

        private async Task<CodeOwnerConfig> GetConfigForDirectory(IFileProvider provider, string directory)
        {
            var directoryContents = provider.GetDirectoryContents(directory);
            if (!directoryContents.Exists) return null;
            
            var codeOwnersFileInfo = provider.GetFileInfo(Path.Combine(directory, "CODEOWNERS"));
            if (!codeOwnersFileInfo.Exists) return null;

            using(var stream = codeOwnersFileInfo.CreateReadStream())
            using(var reader = new StreamReader(stream))
            {
                return await _parser.ParserConfig(await reader.ReadToEndAsync());
            }
        }
    }
}