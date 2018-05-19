using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Types
{
    public class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("username")]
        public string Username { get; set; }
    }
}