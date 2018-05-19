namespace Bulwark.Integration.GitLab
{
    public class GitLabOptions
    {
        public GitLabOptions()
        {
            Enabled = false;
            ServerUrl = "http://192.168.0.6/";
            AuthenticationToken = "AdNAuSLZxGvU1cHycNxU";
            TargetBranchesFilter = "master";
            AutoMergePullRequests = false;
        }
        
        public bool Enabled { get; set; }
        
        public string ServerUrl { get; set; }
        
        public string AuthenticationToken { get; set; }
        
        public string TargetBranchesFilter { get; set; }
        
        public bool AutoMergePullRequests { get; set; }
    }
}