using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Types
{
    public class Commit
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        //[JsonProperty("timestamp")]
        //public DateTime Timestamp { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("author")]
        public Types.Author Author { get; set; }

        [JsonProperty("added")]
        public List<string> Added { get; set; }
        
        [JsonProperty("modified")]
        public List<string> Modified { get; set; }
        
        [JsonProperty("removed")]
        public List<string> Removed { get; set; }
        
        public class Types
        {
            public class Author
            {
                [JsonProperty("name")]
                public string Name { get; set; }
        
                [JsonProperty("email")]
                public string Email { get; set; }
            }
        }
    }
}