using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Types
{
    public class ProjectPublic
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("name_with_namespace")]
        public string NameWithNamespace { get; set; }
        
        [JsonProperty("path")]
        public string Path { get; set; }
        
        [JsonProperty("path_with_namespace")]
        public string PathWithNamespace { get; set; }
            
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("default_branch")]
        public string DefaultBranch { get; set; }
                
        [JsonProperty("tag_list")]
        public List<string> TagList { get; set; }
                
        [JsonProperty("ssh_url_to_repo")]
        public string SshUrlToRepo { get; set; }
        
        [JsonProperty("http_url_to_repo")]
        public string HttpUrlToRepo { get; set; }
                
        [JsonProperty("web_url")]
        public string WebUrl { get; set; }
        
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
                
        [JsonProperty("star_count")]
        public int StarCount { get; set; }
        
        [JsonProperty("forks_count")]
        public int ForksCount { get; set; }
        
        [JsonProperty("last_activity_at")]
        public DateTime LastActivityAt { get; set; }
    }
}