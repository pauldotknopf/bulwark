using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Types
{
    public class Label
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("color")]
        public string Color { get; set; }
        
        [JsonProperty("project_id")]
        public int ProjectId { get; set; }
        
        //[JsonProperty("created_at")]
        //public DateTime CreatedAt { get; set; }
        
        //[JsonProperty("updated_at")]
        //public DateTime UpdatedAt { get; set; }
   
        [JsonProperty("template")]
        public bool Template { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("group_id")]
        public int? GroupId { get; set; }
    }
}