using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bulwark.Strategy.CodeOwners;

namespace Bulwark.Tests.CodeOwners
{
    public static class Helpers
    {
        public static void WriteCodeOwners(string path, CodeOwnerConfig codeOwners)
        {
            var content = new StringBuilder();
            foreach (var entry in codeOwners.Entries)
            {
                content.AppendLine($"{entry.Pattern} {string.Join(" ", entry.Users)}");
            }

            var parentDirectory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parentDirectory))
            {
                if (!Directory.Exists(parentDirectory))
                    Directory.CreateDirectory(parentDirectory);
            }
            if(File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path, content.ToString());
        }
        
        public static void WriteCodeOwnersEntry(string path, string pattern, params string[] users)
        {
            WriteCodeOwners(path, new CodeOwnerConfig
            {
                Entries = new List<CodeOwnerConfig.Entry>
                {
                    new CodeOwnerConfig.Entry
                    {
                        Pattern = pattern,
                        Users = users.ToList()
                    }
                }
            });
        }
    }
}