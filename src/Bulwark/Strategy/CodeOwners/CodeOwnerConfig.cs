using System.Collections.Generic;

namespace Bulwark.Strategy.CodeOwners
{
    public class CodeOwnerConfig
    {
        public CodeOwnerConfig()
        {
            Entries = new List<Entry>();
        }
        
        public List<Entry> Entries { get; set; }
        
        public class Entry
        {
            public string Pattern { get; set; }
            
            public List<string> Users { get; set; }
        }
    }
}