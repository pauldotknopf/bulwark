using Bulwark.Integration.GitLab.Api.Types;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Requests
{
    public class ChangeApprovalConfigurationRequest
    {
        [JsonIgnore]
        public int ProjectId { get; set; }
        
        [JsonIgnore]
        public int MergeRequestIid { get; set; }
        
        [JsonProperty("approvals_required")]
        public int ApprovalsRequired { get; set; }
    }

    public class ChangeApprovalConfigurationResponse : MergeRequestApprovals
    {
        
    }
}