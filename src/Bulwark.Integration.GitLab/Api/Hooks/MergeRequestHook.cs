using System.Collections.Generic;
using Bulwark.Integration.GitLab.Api.Types;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Hooks
{
    public class MergeRequestHook : Hook
    {
        [JsonProperty("event_type")]
        public string EventType { get; set; }
        
        [JsonProperty("user")]
        public MergeRequsetHookTypes.User User { get; set; }
        
        [JsonProperty("repository")]
        public MergeRequsetHookTypes.Repository Repository { get; set; }
        
        [JsonProperty("object_attributes")]
        public MergeRequsetHookTypes.ObjectAttributes ObjectAttributes { get; set; }
        
        [JsonProperty("labels")]
        public List<Label> Labels { get; set; }

        public class MergeRequsetHookTypes
        {
            public class ObjectAttributes
            {
                [JsonProperty("id")]
                public int Id { get; set; }
                
                [JsonProperty("iid")]
                public int Iid { get; set; }
                
                [JsonProperty("target_branch")]
                public string TargetBranch { get; set; }
                
                [JsonProperty("source_branch")]
                public string SourceBranch { get; set; }
                
                [JsonProperty("source_project_id")]
                public int SourceProjectId { get; set; }
                
                [JsonProperty("author_id")]
                public int AuthorId { get; set; }
                
                [JsonProperty("assignee_id")]
                public int? AssigneeId { get; set; }
                
                [JsonProperty("title")]
                public string Title { get; set; }
                
                //[JsonProperty("created_at")]
                //public DateTime CreatedAt { get; set; }
                
                //[JsonProperty("updated_at")]
                //public DateTime UpdatedAt { get; set; }
                
                [JsonProperty("milestone_id")]
                public string MilestoneId { get; set; }
                
                [JsonProperty("state")]
                public MergeRequestState State { get; set; }
                
                [JsonProperty("merge_status")]
                public MergeRequestStatus MergeStatus { get; set; }
                
                [JsonProperty("target_project_id")]
                public int TargetProjectId { get; set; }
                
                [JsonProperty("description")]
                public string Description { get; set; }
                
                [JsonProperty("source")]
                public HookTypes.Project Source { get; set; }
                
                [JsonProperty("target")]
                public HookTypes.Project Target { get; set; }
                
                [JsonProperty("last_commit")]
                public Commit LastCommit { get; set; }
                
                [JsonProperty("work_in_progress")]
                public bool WorkInProgress { get; set; }
                
                [JsonProperty("url")]
                public string Url { get; set; }
                
                [JsonProperty("action")]
                public string Action { get; set; }
                
                [JsonProperty("assignee")]
                public User Assignee { get; set; }
            }
            
            public class User
            {
                [JsonProperty("name")]
                public string Name { get; set; }
                
                [JsonProperty("username")]
                public string Username { get; set; }
                
                [JsonProperty("avatar_url")]
                public string AvatarUrl { get; set; }
            }

            public class Repository
            {
                [JsonProperty("name")]
                public string Name { get; set; }
                
                [JsonProperty("url")]
                public string Url { get; set; }
                
                [JsonProperty("description")]
                public string Description { get; set; }
                
                [JsonProperty("homepage")]
                public string Homepage { get; set; }
            }
        }
    }
}