﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Bulwark.Integration.GitLab.Api.Requests;
using Bulwark.Integration.GitLab.Api.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Impl
{
    public class GitLabApi : IGitLabApi
    {
        readonly ILogger<GitLabApi> _logger;
        readonly GitLabOptions _options;
        
        public GitLabApi(IOptions<GitLabOptions> options,
            ILogger<GitLabApi> logger)
        {
            _logger = logger;
            _options = options.Value;
            if (string.IsNullOrEmpty(_options.ServerUrl))
            {
                throw new Exception("You must provide a GitLab server url.");
            }

            if (string.IsNullOrEmpty(_options.AuthenticationToken))
            {
                throw new Exception("You must provide a GitLab authentication token.");
            }
        }

        public Task<MergeRequestsResponse> GetMergeRequests(MergeRequestsRequest request)
        {
            var queryParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(request));
            return Get<MergeRequestsResponse>(
                request.ProjectId.HasValue ? 
                    $"/api/v4/projects/{request.ProjectId}/merge_requests" : 
                    "/api/v4/merge_requests",
                queryParameters);
        }
        
        public Task<MergeRequest> GetMergeRequest(int projectId, int mergeRequestIid)
        {
            return Get<MergeRequest>($"/api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}");
        }

        public Task<MergeRequestApprovals> GetMergeRequestApprovals(int projectId, int mergeRequestIid)
        {
            return Get<MergeRequestApprovals>($"/api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}/approvals");
        }

        public Task<AcceptMergeRequestResponse> AcceptMergeRequest(AcceptMergeRequestRequest request)
        {
            return Put<AcceptMergeRequestResponse>(
                $"/api/v4/projects/{request.ProjectId}/merge_requests/{request.MergeRequestIid}/merge",
                request);
        }

        public Task<ChangeApprovalConfigurationResponse> UpdateMergeRequestApprovals(ChangeApprovalConfigurationRequest request)
        {
            return Post<ChangeApprovalConfigurationResponse>(
                $"/api/v4/projects/{request.ProjectId}/merge_requests/{request.MergeRequestIid}/approvals",
                request);
        }

        public Task<UpdateApproversResponse> UpdateMergeRequestAllowApprovers(UpdateApproversRequest request)
        {
            return Put<UpdateApproversResponse>(
                $"/api/v4/projects/{request.ProjectId}/merge_requests/{request.MergeRequestIid}/approvers",
                request);
        }

        public Task<List<User>> GetUsers(UsersRequest request)
        {
            var url = "/api/v4/users";
            
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(request.Username))
                query["username"] = request.Username;

            if (query.Count > 0)
            {
                url += $"?{query}";
            }
            
            return Get<List<User>>(url);
        }

        public Task<ProjectResponse> GetProject(ProjectRequest request)
        {
            return Get<ProjectResponse>($"/api/v4/projects/{request.ProjectId}");
        }

        private async Task<T> Post<T>(string url, object data)
        {
            _logger.LogTrace("Post: {Url} with data: {@Data}", url, data);
            
            using (var client = GetClient())
            {
                using (var input = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"))
                {
                    using (var response = await client.PostAsync(url, input))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var output = response.Content)
                        {
                            var json = await output.ReadAsStringAsync();
                            _logger.LogTrace("Response: {Json}", json);
                            return JsonConvert.DeserializeObject<T>(json);
                        }
                    }
                }
            }
        }
        
        private async Task<T> Put<T>(string url, object data)
        {
            _logger.LogTrace("Put: {Url} with data: {@Data}", url, data);
            
            using (var client = GetClient())
            {
                using (var input = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"))
                {
                    using (var response = await client.PutAsync(url, input))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var output = response.Content)
                        {
                            var json = await output.ReadAsStringAsync();
                            _logger.LogTrace("Response: {Json}", json);
                            return JsonConvert.DeserializeObject<T>(json);
                        }
                    }
                }
            }
        }
        
        private async Task<T> Get<T>(string url, Dictionary<string, string> queryParameters = null)
        {
            if (queryParameters != null && queryParameters.Count > 0)
            {
                var q = HttpUtility.ParseQueryString(string.Empty);
                foreach (var entry in queryParameters)
                {
                    q.Add(entry.Key, entry.Value);
                }

                url += $"?{q}";
            }
            
            _logger.LogTrace("Get: {Url}", url);
            
            using (var client = GetClient())
            {
                using (var response = await client.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    using (var content = response.Content)
                    {
                        var json = await content.ReadAsStringAsync();
                        _logger.LogTrace("Response: {Json}", json);
                        return JsonConvert.DeserializeObject<T>(json);
                    }
                }
            }
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(_options.ServerUrl);
            client.DefaultRequestHeaders.Add("Private-Token", _options.AuthenticationToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}