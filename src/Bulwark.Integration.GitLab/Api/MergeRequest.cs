using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api
{
    public class MergeRequest
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("iid")]
        public int Iid { get; set; }
        
        [JsonProperty("project_id")]
        public int ProjectId { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("opened")]
        public string State { get; set; }
        
        //[JsonProperty("created_at")]
        //public DateTime CreatedAt { get; set; }
        
        //[JsonProperty("updated_at")]
        //public DateTime UpdatedAt { get; set; }
        
        [JsonProperty("target_branch")]
        public string TargetBranch { get; set; }
        
        [JsonProperty("source_branch")]
        public string SourceBranch { get; set; }
        
        [JsonProperty("upvotes")]
        public int Upvotes { get; set; }
        
        [JsonProperty("downvotes")]
        public int Downvotes { get; set; }
        
        [JsonProperty("author")]
        public MergeRequestUser Author { get; set; }
        
        [JsonProperty("assignee")]
        public MergeRequestUser Assignee { get; set; }
        
        [JsonProperty("source_project_id")]
        public int SourceProjectId { get; set; }
        
        [JsonProperty("target_project_id")]
        public int TargetProjectId { get; set; }
        
        [JsonProperty("labels")]
        public List<string> Labels { get; set; }
        
        [JsonProperty("work_in_progress")]
        public bool WorkInProgress { get; set; }
        
        [JsonProperty("milestone")]
        public Milestone Milestone { get; set; }
        
        [JsonProperty("merge_when_build_succeeds")]
        public bool MergeWhenBuildSucceeds { get; set; }
        
        [JsonProperty("merge_status")]
        public MergeStatus MergeStatus { get; set; }
        
        [JsonProperty("sha")]
        public string Sha { get; set; }
        
        [JsonProperty("merge_commit_sha")]
        public string MergeCommitSha { get; set; }
        
        [JsonProperty("subscribed")]
        public bool Subscribed { get; set; }
        
        [JsonProperty("user_notes_count")]
        public int UserNotesCount { get; set; }
        
        [JsonProperty("approvals_before_merge")]
        public int? ApprovalsBeforeMerge { get; set; }
    }
}