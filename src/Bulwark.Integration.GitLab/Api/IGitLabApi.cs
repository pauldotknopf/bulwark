using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bulwark.Integration.GitLab.Api
{
    public interface IGitLabApi
    {
        Task<MergeRequest> GetMergeRequest(int projectId, int mergeRequestIid);

        Task<MergeRequestApprovals> GetMergeRequestApprovals(int projectId, int mergeRequestIid);

        Task<UpdateApproversResponse> UpdateMergeRequestAllowApprovers(UpdateApproversRequest request);
    }
}