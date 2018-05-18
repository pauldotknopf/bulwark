using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Hooks
{
    public class User
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("username")]
        public string Username { get; set; }
        
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
    }
    
    public class MergeRequest
    {
        [JsonProperty("merge_request")]
        public string ObjectKind { get; set; }
        
        [JsonProperty("user")]
        public User User { get; set; }
        
        [JsonProperty("project")]
        public Project Project { get; set; }
        
        [JsonProperty("repository")]
        public Repository Repository { get; set; }
        
        [JsonProperty("object_attributes")]
        public ObjectAttributes ObjectAttributes { get; set; }
        
        [JsonProperty("labels")]
        public List<Label> Labels { get; set; }
    }

    public class Project
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("web_url")]
        public string WebUrl { get; set; }
        
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
        
        [JsonProperty("git_ssh_url")]
        public string GitSshUrl { get; set; }
        
        [JsonProperty("git_http_url")]
        public string GitHttpUrl { get; set; }
        
        [JsonProperty("namespace")]
        public string Namespace { get; set; }
        
        [JsonProperty("visibility_level")]
        public int VisibilityLevel { get; set; }
        
        [JsonProperty("path_with_namespace")]
        public string PathWithNamespace { get; set; }
        
        [JsonProperty("default_branch")]
        public string DefaultBranch { get; set; }
        
        [JsonProperty("homepage")]
        public string Homepage { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("ssh_url")]
        public string SshUrl { get; set; }
        
        [JsonProperty("http_url")]
        public string HttpUrl { get; set; }
    }

    public class Repository
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("Url")]
        public string Url { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("homepage")]
        public string Homepage { get; set; }
    }

    public class ObjectAttributes
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
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
        public string State { get; set; }
        
        [JsonProperty("merge_status")]
        public string MergeStatus { get; set; }
        
        [JsonProperty("target_project_id")]
        public int TargetProjectId { get; set; }
        
        [JsonProperty("iid")]
        public int Iid { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("source")]
        public Project Source { get; set; }
        
        [JsonProperty("target")]
        public Project Target { get; set; }
        
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

    public class Commit
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        //[JsonProperty("timestamp")]
        //public DateTime Timestamp { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("author")]
        public Author Author { get; set; }
    }

    public class Author
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("email")]
        public string Email { get; set; }
    }

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
        public int GroupId { get; set; }
    }
}