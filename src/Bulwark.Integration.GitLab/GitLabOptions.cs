namespace Bulwark.Integration.GitLab
{
    public class GitLabOptions
    {
        public GitLabOptions()
        {
            Enabled = false;
            RepositoryCacheLocation = "repository-cache";
        }
        
        public bool Enabled { get; set; }
        
        public string RepositoryCacheLocation { get; set; }
    }
}