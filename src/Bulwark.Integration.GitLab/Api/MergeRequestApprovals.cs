using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api
{
    public class MergeRequestApprovals
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("iid")]
        public int Iid { get; set; }
        
        [JsonProperty("project_id")]
        public int ProjectId { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("state")]
        public string State { get; set; }
        
        //[JsonProperty("created_at")]
        //public DateTime CreatedAt { get; set; }
        
        //[JsonProperty("updated_at")]
        //public DateTime UpdatedAt { get; set; }
        
        [JsonProperty("merge_status")]
        public MergeStatus MergeStatus { get; set; }
        
        [JsonProperty("approvals_required")]
        public int ApprovalsRequired { get; set; }
        
        [JsonProperty("approvals_left")]
        public int ApprovalsLeft { get; set; }
        
        [JsonProperty("approved_by")]
        public List<MergeRequestApprover> ApprovedBy { get; set; }
        
        [JsonProperty("suggested_approvers")]
        public List<MergeRequestUser> SuggestedApprovers { get; set; }
        
        [JsonProperty("approvers")]
        public List<MergeRequestApprover> Approvers { get; set; }
        
        [JsonProperty("approver_groups")]
        public List<MergeRequestApprover> ApproverGroups { get; set; }
        
        [JsonProperty("user_has_approved")]
        public bool UserHasApproved { get; set; }
        
        [JsonProperty("user_can_approve")]
        public bool UserCanApprove { get; set; }
    }

    public class MergeRequestApprover
    {
        [JsonProperty("user")]
        public MergeRequestUser User { get; set; }
        
        [JsonProperty("group")]
        public MergeRequestGroup Group { get; set; }
    }

    public class MergeRequestGroup
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("path")]
        public string Path { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("visibility")]
        public string Visibility { get; set; }
        
        [JsonProperty("lfs_enabled")]
        public bool LfsEnabled { get; set; }
        
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
        
        [JsonProperty("web_url")]
        public string WebUrl { get; set; }
        
        [JsonProperty("request_access_enabled")]
        public bool RequestAccessEnabled { get; set; }
        
        [JsonProperty("full_name")]
        public string FullName { get; set; }
        
        [JsonProperty("full_path")]
        public string FullPath { get; set; }
        
        [JsonProperty("parent_id")]
        public int? ParentId { get; set; }
        
        [JsonProperty("ldap_cn")]
        public string LdapCn { get; set; }
        
        [JsonProperty("ldap_access")]
        public string LdapAccess { get; set; }
    }

    public class MergeRequestUser
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("username")]
        public string Username { get; set; }
        
        [JsonProperty("state")]
        public string State { get; set; }
        
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
        
        [JsonProperty("web_url")]
        public string WebUrl { get; set; }
    }
}