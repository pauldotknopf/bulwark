using Bulwark.Integration.GitLab.Api.Types;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Requests
{
    public class AcceptMergeRequestRequest
    {
        [JsonIgnore]
        public int ProjectId { get; set; }
        
        [JsonIgnore]
        public int MergeRequestIid { get; set; }
        
        [JsonProperty("merge_commit_message", NullValueHandling = NullValueHandling.Ignore)]
        public string MergeCommitMessage { get; set; }
        
        [JsonProperty("should_remove_source_branch", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShouldRemoveSourceBranch { get; set; }
        
        [JsonProperty("merge_when_pipeline_succeeds", NullValueHandling = NullValueHandling.Ignore)]
        public bool? MergeWhenPipelineSuceeds { get; set; }
        
        [JsonProperty("sha", NullValueHandling = NullValueHandling.Ignore)]
        public string Sha { get; set; }
    }

    public class AcceptMergeRequestResponse : MergeRequest
    {
        
    }
}