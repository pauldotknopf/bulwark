namespace Bulwark.Integration.GitLab
{
    public class GitLabOptions
    {
        public GitLabOptions()
        {
            Enabled = true;
            ServerUrl = "https://gitlab.com/";
            UseHttp = true;
        }
        
        public bool Enabled { get; set; }
        
        public string ServerUrl { get; set; }
        
        public string AuthenticationToken { get; set; }
        
        public string SecretToken { get; set; }
        
        public string TargetBranchesFilter { get; set; }
        
        public bool AutoMergePullRequests { get; set; }
        
        public string MergeCommitMessage { get; set; }
        
        public bool? MergeWhenPipelineSuceeds { get; set; }
        
        public bool? ShouldRemoveSourceBranch { get; set; }
        
        public bool UseHttp { get; set; }
        
        public string HttpUsername { get; set; }
        
        public string HttpPassword { get; set; }
    }
}