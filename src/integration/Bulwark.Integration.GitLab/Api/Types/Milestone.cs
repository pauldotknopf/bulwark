using System;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Types
{
    public class Milestone
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("iid")]
        public int Iid { get; set; }
        
        [JsonProperty("group_id")]
        public int GroupId { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
    
        [JsonProperty("state")]
        public string State { get; set; }
        
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        [JsonProperty("due_date")]
        public DateTime? DueDate { get; set; }
        
        [JsonProperty("start_date")]
        public DateTime? StartDate { get; set; }
    }
}