﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Api.Impl
{
    public class GitLabApi : IGitLabApi
    {
        readonly GitLabOptions _options;
        
        public GitLabApi(IOptions<GitLabOptions> options)
        {
            _options = options.Value;
        }
        
        public Task<MergeRequest> GetMergeRequest(int projectId, int mergeRequestIid)
        {
            return Get<MergeRequest>($"/api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}");
        }

        public Task<MergeRequestApprovals> GetMergeRequestApprovals(int projectId, int mergeRequestIid)
        {
            return Get<MergeRequestApprovals>($"/api/v4/projects/{projectId}/merge_requests/{mergeRequestIid}/approvals");
        }

        public Task<UpdateApproversResponse> UpdateMergeRequestAllowApprovers(UpdateApproversRequest request)
        {
            return Put<UpdateApproversResponse>(
                $"/api/v4/projects/{request.ProjectId}/merge_requests/{request.MergeRequestIid}/approvers",
                request);
        }

        private async Task<T> Put<T>(string url, object data)
        {
            using (var client = GetClient())
            {
                using (var input = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"))
                {
                    using (var response = await client.PutAsync(url, input))
                    {
                        using (var output = response.Content)
                        {
                            var json = await output.ReadAsStringAsync();
                            Debug.WriteLine(json);
                            return JsonConvert.DeserializeObject<T>(json);
                        }
                    }
                }
            }
        }
        
        private async Task<T> Get<T>(string url)
        {
            using (var client = GetClient())
            {
                using (var response = await client.GetAsync(url))
                {
                    using (var content = response.Content)
                    {
                        var json = await content.ReadAsStringAsync();
                        Debug.WriteLine(json);
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