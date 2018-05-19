using System.Collections.Generic;
using System.Threading.Tasks;
using Bulwark.Integration.GitLab.Api.Requests;
using Bulwark.Integration.GitLab.Api.Types;

namespace Bulwark.Integration.GitLab.Api
{
    public interface IGitLabApi
    {
        Task<MergeRequestsResponse> GetMergeRequests(MergeRequestsRequest request);
        
        Task<MergeRequest> GetMergeRequest(int projectId, int mergeRequestIid);

        Task<MergeRequestApprovals> GetMergeRequestApprovals(int projectId, int mergeRequestIid);

        Task<ChangeApprovalConfigurationResponse> UpdateMergeRequestApprovals(ChangeApprovalConfigurationRequest request);
        
        Task<UpdateApproversResponse> UpdateMergeRequestAllowApprovers(UpdateApproversRequest request);

        Task<List<User>> GetUsers(UsersRequest request);

        Task<ProjectResponse> GetProject(ProjectRequest request);
    }
}