using System.Collections.Generic;
using Bulwark.Integration.GitLab.Api.Types;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Hooks
{
    public class PushHook : Hook
    {
        [JsonProperty("event_name")]
        public string EventName { get; set; }
        
        [JsonProperty("before")]
        public string Before { get; set; }
        
        [JsonProperty("after")]
        public string After { get; set; }
        
        [JsonProperty("ref")]
        public string Ref { get; set; }
        
        [JsonProperty("checkout_sha")]
        public string CheckoutSha { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("user_id")]
        public int UserId { get; set; }
        
        [JsonProperty("user_name")]
        public string UserName { get; set; }
        
        [JsonProperty("user_username")]
        public string UserUsername { get; set; }
        
        [JsonProperty("user_email")]
        public string UserEmail { get; set; }
        
        [JsonProperty("user_avatar")]
        public string UserAvatar { get; set; }
        
        [JsonProperty("project_id")]
        public int ProjectId { get; set; }
        
        [JsonProperty("commits")]
        public List<Commit> Commits { get; set; }
        
        [JsonProperty("total_commits_count")]
        public int TotalCommitsCount { get; set; }
    }
}