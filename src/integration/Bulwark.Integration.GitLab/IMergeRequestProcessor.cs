using System.Threading.Tasks;

namespace Bulwark.Integration.GitLab
{
    public interface IMergeRequestProcessor
    {
        Task ProcessMergeRequest(int projectId, int mergeRequestIid);
    }
}