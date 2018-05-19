using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bulwark.Integration.GitLab.Api.Types
{
    public class Project : ProjectPublic
    {
        [JsonProperty("archived")]
        public bool Archived { get; set; }
        
        [JsonProperty("")]
        public Types.Visibility Visibility { get; set; }

        [JsonProperty("resolve_outdated_diff_discussions")]
        public bool ResolveOutdatedDiffDiscussions { get; set; }

        [JsonProperty("container_registry_enabled")]
        public bool ContainerRegistryEnabled { get; set; }
        
        [JsonProperty("issues_enabled")]
        public bool IssuesEnabled { get; set; }
        
        [JsonProperty("merge_requests_enabled")]
        public bool MergeRequestsEnabled { get; set; }
        
        [JsonProperty("wiki_enabled")]
        public bool WikiEnabled { get; set; }
        
        [JsonProperty("jobs_enabled")]
        public bool JobsEnabled { get; set; }
        
        [JsonProperty("snippets_enabled")]
        public bool SnippetsEnabled { get; set; }
        
        [JsonProperty("shared_runners_enabled")]
        public bool SharedRunnersEnabled { get; set; }
        
        [JsonProperty("lfs_enabled")]
        public bool LfsEnabled { get; set; }
        
        [JsonProperty("creator_id")]
        public int CreatorId { get; set; }
        
        [JsonProperty("namespace")]
        public Types.Namespace Namespace { get; set; }
        
        [JsonProperty("import_status")]
        public string ImportStatus { get; set; }
        
        [JsonProperty("import_error")]
        public string ImportError { get; set; }
        
        [JsonProperty("open_issues_count")]
        public int OpenIssuesCount { get; set; }
        
        [JsonProperty("runners_token")]
        public string RunnersToken { get; set; }
        
        [JsonProperty("public_jobs")]
        public bool PublicJobs { get; set; }
        
        [JsonProperty("ci_config_path")]
        public string CiConfigPath { get; set; }
        
        [JsonProperty("shared_with_groups")]
        public List<Types.Group> SharedWithGroups { get; set; }
        
        [JsonProperty("only_allow_merge_if_pipeline_succeeds")]
        public bool OnlyAllowMergeIfPipelineSucceeds { get; set; }
        
        [JsonProperty("request_access_enabled")]
        public bool RequestAccessEnabled { get; set; }
        
        [JsonProperty("only_allow_merge_if_all_discussions_are_resolved")]
        public bool OnlyAllowMergeIfAllDiscussionsAreResolved { get; set; }
        
        [JsonProperty("printing_merge_request_link_enabled")]
        public bool PrintingMergeRequestLinkEnabled { get; set; }
        
        [JsonProperty("merge_method")]
        public string MergeMethod { get; set; }
        
        //  "permissions": {
        //    "project_access": null,
        //    "group_access": {
        //      "access_level": 50,
        //      "notification_level": 3
        //    }
        //  },
        
        [JsonProperty("repository_storage")]
        public string RepositoryStorage { get; set; }
        
        [JsonProperty("approvals_before_merge")]
        public int? ApprovalsBeforeMerge { get; set; }
        
        public class Types
        {
            public class Group
            {
                [JsonProperty("group_id")]
                public int Id { get; set; }
                
                [JsonProperty("group_name")]
                public string GroupName { get; set; }
                
                [JsonProperty("group_access_level")]
                public int GroupAccessLevel { get; set; }
            }
            
            [JsonConverter(typeof(StringEnumConverter))]
            public enum Visibility
            {
                [EnumMember(Value="public")]
                Public,
                [EnumMember(Value="private")]
                Private,
                [EnumMember(Value = "internal")]
                Internal
            }

            public class Namespace
            {
                [JsonProperty("id")]
                public int Id { get; set; }
                
                [JsonProperty("name")]
                public string Name { get; set; }
                
                [JsonProperty("path")]
                public string Path { get; set; }
                
                [JsonProperty("kind")]
                public string Kind { get; set; }
                
                [JsonProperty("full_path")]
                public string FullPath { get; set; }
                
                [JsonProperty("parent_id")]
                public int? ParentId { get; set; }
            }
        }
    }
}