#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using SimpleIdentityServer.Client.Configuration;
using SimpleIdentityServer.Client.DTOs.Responses;
using SimpleIdentityServer.Client.Extensions;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Policy
{
    public interface IPolicyClient
    {
        Task<AddPolicyResponse> Add(PostPolicy request, string url, string token);
        Task<AddPolicyResponse> AddByResolution(PostPolicy request, string url, string token);

        Task<PolicyResponse> GetPolicyAsync(
            string policyId,
            string policyUrl,
            string authorizationHeaderValue);

        Task<PolicyResponse> GetPolicyByResolvingUrlAsync(
            string policyId,
            string configurationUrl,
            string authorizationHeaderValue);

        Task<PolicyResponse> GetPolicyByResolvingUrlAsync(
            string policyId,
            Uri configurationUri,
            string authorizationHeaderValue);
        
        Task<bool> DeletePolicyAsync(
            string policyId,
            string policyUrl,
            string authorizationHeaderValue);

        Task<bool> DeletePolicyByResolvingUrlAsync(
            string policyId,
            string configurationUrl,
            string authorizationHeaderValue);

        Task<bool> DeletePolicyByResolvingUrlAsync(
            string policyId,
            Uri configurationUri,
            string authorizationHeaderValue);

        Task<List<string>> GetPoliciesAsync(
            string policyUrl, 
            string authorizationHeaderValue);

        Task<List<string>> GetPoliciesAsync(
            Uri policyUri,
            string authorizationHeaderValue);

        Task<List<string>> GetPoliciesByResolvingUrlAsync(
            string configurationUrl, 
            string authorizationHeaderValue);

        Task<List<string>> GetPoliciesByResolvingUrlAsync(
            Uri configurationUri, 
            string authorizationHeaderValue);
    }

    internal class PolicyClient : IPolicyClient
    {
        private readonly IAddPolicyOperation _addPolicyOperation;
        private readonly IGetPolicyOperation _getPolicyOperation;
        private readonly IDeletePolicyOperation _deletePolicyOperation;
        private readonly IGetPoliciesOperation _getPoliciesOperation;
        private readonly IGetConfigurationOperation _getConfigurationOperation;

        public PolicyClient(
            IAddPolicyOperation addPolicyOperation,
            IGetPolicyOperation getPolicyOperation,
            IDeletePolicyOperation deletePolicyOperation,
            IGetPoliciesOperation getPoliciesOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _addPolicyOperation = addPolicyOperation;
            _getPolicyOperation = getPolicyOperation;
            _deletePolicyOperation = deletePolicyOperation;
            _getPoliciesOperation = getPoliciesOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        public Task<AddPolicyResponse> Add(PostPolicy request, string url, string token)
        {
            return _addPolicyOperation.ExecuteAsync(request, url, token);
        }

        public async Task<AddPolicyResponse> AddByResolution(PostPolicy request, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url));
            return await Add(request, policyEndpoint, token);
        }

        public async Task<PolicyResponse> GetPolicyAsync(string policyId, string policyUrl, string authorizationHeaderValue)
        {
            return await _getPolicyOperation.ExecuteAsync(policyId, policyUrl, authorizationHeaderValue);
        }

        public async Task<PolicyResponse> GetPolicyByResolvingUrlAsync(string policyId, string configurationUrl, string authorizationHeaderValue)
        {
            return await GetPolicyByResolvingUrlAsync(policyId, UriHelpers.GetUri(configurationUrl), authorizationHeaderValue);
        }

        public async Task<PolicyResponse> GetPolicyByResolvingUrlAsync(string policyId, Uri configurationUri, string authorizationHeaderValue)
        {
            var policyEndpoint = await GetPolicyEndPoint(configurationUri);
            return await GetPolicyAsync(policyId, policyEndpoint, authorizationHeaderValue);
        }
        
        public async Task<bool> DeletePolicyAsync(string policyId, string policyUrl, string authorizationHeaderValue)
        {
            return await _deletePolicyOperation.ExecuteAsync(policyId, policyUrl, authorizationHeaderValue);
        }

        public async Task<bool> DeletePolicyByResolvingUrlAsync(string policyId, string configurationUrl, string authorizationHeaderValue)
        {
            return await DeletePolicyByResolvingUrlAsync(policyId, UriHelpers.GetUri(configurationUrl), authorizationHeaderValue);
        }

        public async Task<bool> DeletePolicyByResolvingUrlAsync(string policyId, Uri configurationUri, string authorizationHeaderValue)
        {
            var policyEndpoint = await GetPolicyEndPoint(configurationUri);
            return await DeletePolicyAsync(policyId, policyEndpoint, authorizationHeaderValue);
        }

        public async Task<List<string>> GetPoliciesAsync(string policyUrl, string authorizationHeaderValue)
        {
            return await GetPoliciesAsync(UriHelpers.GetUri(policyUrl), authorizationHeaderValue);
        }

        public async Task<List<string>> GetPoliciesAsync(Uri policyUri, string authorizationHeaderValue)
        {
            return await _getPoliciesOperation.ExecuteAsync(policyUri, authorizationHeaderValue);
        }

        public async Task<List<string>> GetPoliciesByResolvingUrlAsync(string configurationUrl, string authorizationHeaderValue)
        {
            return await GetPoliciesByResolvingUrlAsync(UriHelpers.GetUri(configurationUrl), authorizationHeaderValue);
        }

        public async Task<List<string>> GetPoliciesByResolvingUrlAsync(Uri configurationUri, string authorizationHeaderValue)
        {
            var policyEndpoint = await GetPolicyEndPoint(configurationUri);
            return await GetPoliciesAsync(policyEndpoint, authorizationHeaderValue);
        }
        
        private async Task<string> GetPolicyEndPoint(Uri configurationUri)
        {
            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri);
            return configuration.PolicyEndPoint;
        }
    }
}
