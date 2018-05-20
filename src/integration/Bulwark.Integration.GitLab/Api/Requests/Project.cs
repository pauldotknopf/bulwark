using Bulwark.Integration.GitLab.Api.Types;

namespace Bulwark.Integration.GitLab.Api.Requests
{
    public class ProjectRequest
    {
        public int ProjectId { get; set; }
    }

    public class ProjectResponse : Project
    {
        
    }
}