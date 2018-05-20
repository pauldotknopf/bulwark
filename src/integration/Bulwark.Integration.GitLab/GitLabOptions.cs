namespace Bulwark.Integration.GitLab
{
    public class GitLabOptions
    {
        public bool Enabled { get; set; }
        
        public string ServerUrl { get; set; }
        
        public string AuthenticationToken { get; set; }
        
        public string TargetBranchesFilter { get; set; }
        
        public bool AutoMergePullRequests { get; set; }
        
        public string MergeCommitMessage { get; set; }
        
        public bool? MergeWhenPipelineSuceeds { get; set; }
        
        public bool? ShouldRemoveSourceBranch { get; set; }
    }
}