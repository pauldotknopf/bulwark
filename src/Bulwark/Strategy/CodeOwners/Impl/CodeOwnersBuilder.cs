using System;
using System.Collections.Generic;
using System.IO;
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

            if (string.IsNullOrEmpty(path))
                return result;

            // First, get all parent code files.
            var codeFiles = WalkParentsForCodeOwners(provider, path.StartsWith("/") ? path : $"{Path.DirectorySeparatorChar}{path}");
            var codeFileContent = new StringBuilder();
            foreach (var codeFile in codeFiles)
            {
                using(var stream = codeFile.CreateReadStream())
                using(var reader = new StreamReader(stream))
                {
                    codeFileContent.Append(await reader.ReadToEndAsync());
                }
            }

            var codeOwnerConfig = await _parser.ParserConfig(codeFileContent.ToString());
            
            foreach (var entry in codeOwnerConfig.Entries)
            {
                if(Wildmatch.Match(entry.Pattern.TrimStart('/'), path.TrimStart('/'), MatchFlags.CaseFold) == MatchResult.Match)
                {
                    result.AddRange(entry.Users);
                }
            }
            
            return result;
        }

        private List<IFileInfo> WalkParentsForCodeOwners(IFileProvider fileProvider, string path)
        {
            var result = new List<IFileInfo>();
            
            var codeOwnersFileInfo = fileProvider.GetFileInfo(Path.Combine(path, "CODEOWNERS"));
            if (codeOwnersFileInfo.Exists)
            {
                result.Add(codeOwnersFileInfo);
            }
            
            // Get the code files from parent directories.
            var parentDirectory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parentDirectory)) return result;
            foreach (var child in WalkParentsForCodeOwners(fileProvider, parentDirectory))
            {
                result.Add(child);
            }

            return result;
        }
    }
}