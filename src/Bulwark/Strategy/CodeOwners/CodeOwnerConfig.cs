using System;
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
            public Entry()
            {
                Users = new List<string>();
            }
            
            public string Pattern { get; set; }
            
            public List<string> Users { get; set; }

            public Entry AddUser(string user)
            {
                Users.Add(user);
                return this;
            }
        }

        public CodeOwnerConfig AddEntry(string pattern, Action<Entry> action)
        {
            var entry = new Entry
            {
                Pattern = pattern
            };
            Entries.Add(entry);
            action(entry);
            return this;
        }
    }
}