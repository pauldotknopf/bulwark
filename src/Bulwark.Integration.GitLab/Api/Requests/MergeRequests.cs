using System.Collections.Generic;
using Bulwark.Integration.GitLab.Api.Types;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Requests
{
    public class MergeRequestsRequest
    {
        [JsonIgnore]
        public int? ProjectId { get; set; }
        
        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public MergeRequestState? State { get; set; }
        
        [JsonProperty("source_branch", NullValueHandling = NullValueHandling.Ignore)]
        public string SourceBranch { get; set; }
        
        [JsonProperty("target_branch", NullValueHandling = NullValueHandling.Ignore)]
        public string TargetBranch { get; set; }
    }

    public class MergeRequestsResponse : List<MergeRequest>
    {
        
    }
}