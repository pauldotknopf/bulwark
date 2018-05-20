using System.Collections.Generic;
using Bulwark.Integration.GitLab.Api.Types;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Requests
{
    public class UpdateApproversRequest
    {
        public UpdateApproversRequest()
        {
            ApproverIds = new List<int>();
            ApproverGroupIds = new List<int>();
        }
        
        [JsonIgnore]
        public int ProjectId { get; set; }
        
        [JsonIgnore]
        public int MergeRequestIid { get; set; }
        
        [JsonProperty("approver_ids")]
        public List<int> ApproverIds { get; set; }
        
        [JsonProperty("approver_group_ids")]
        public List<int> ApproverGroupIds { get; set; }
    }

    public class UpdateApproversResponse : MergeRequestApprovals
    {

    }
}