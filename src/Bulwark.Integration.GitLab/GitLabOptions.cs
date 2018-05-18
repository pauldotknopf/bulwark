namespace Bulwark.Integration.GitLab
{
    public class GitLabOptions
    {
        public GitLabOptions()
        {
            Enabled = false;
            RepositoryCacheLocation = "repository-cache";
            ServerUrl = "http://192.168.0.6/";
            AuthenticationToken = "AdNAuSLZxGvU1cHycNxU";
        }
        
        public bool Enabled { get; set; }
        
        public string RepositoryCacheLocation { get; set; }
        
        public string ServerUrl { get; set; }
        
        public string AuthenticationToken { get; set; }
    }
}