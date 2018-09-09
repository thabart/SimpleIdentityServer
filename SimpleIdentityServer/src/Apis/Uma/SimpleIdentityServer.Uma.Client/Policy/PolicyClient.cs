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
using SimpleIdentityServer.Client.Extensions;
using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Uma.Client.Policy;
using SimpleIdentityServer.Uma.Client.Results;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Policy
{
    public interface IPolicyClient
    {
        Task<AddPolicyResult> Add(PostPolicy request, string url, string token);
        Task<AddPolicyResult> AddByResolution(PostPolicy request, string url, string token);
        Task<GetPolicyResult> Get(string id, string url, string token);
        Task<GetPolicyResult> GetByResolution(string id, string url, string token);
        Task<GetPoliciesResult> GetAll(string url, string token);
        Task<GetPoliciesResult> GetAllByResolution(string url, string token);
        Task<BaseResponse> Delete(string id, string url, string token);
        Task<BaseResponse> DeleteByResolution(string id, string url, string token);
        Task<BaseResponse> Update(PutPolicy request, string url, string token);
        Task<BaseResponse> UpdateByResolution(PutPolicy request, string url, string token);
        Task<BaseResponse> AddResource(string id, PostAddResourceSet request, string url, string token);
        Task<BaseResponse> AddResourceByResolution(string id, PostAddResourceSet request, string url, string token);
        Task<BaseResponse> DeleteResource(string id, string resourceId, string url, string token);
        Task<BaseResponse> DeleteResourceByResolution(string id, string resourceId, string url, string token);
        Task<SearchAuthPoliciesResult> ResolveSearch(string url, SearchAuthPolicies parameter, string authorizationHeaderValue = null);
    }

    internal class PolicyClient : IPolicyClient
    {
        private readonly IAddPolicyOperation _addPolicyOperation;
        private readonly IGetPolicyOperation _getPolicyOperation;
        private readonly IDeletePolicyOperation _deletePolicyOperation;
        private readonly IGetPoliciesOperation _getPoliciesOperation;
        private readonly IAddResourceToPolicyOperation _addResourceToPolicyOperation;
        private readonly IDeleteResourceFromPolicyOperation _deleteResourceFromPolicyOperation;
        private readonly IUpdatePolicyOperation _updatePolicyOperation;
        private readonly IGetConfigurationOperation _getConfigurationOperation;
        private readonly ISearchPoliciesOperation _searchPoliciesOperation;

        public PolicyClient(
            IAddPolicyOperation addPolicyOperation,
            IGetPolicyOperation getPolicyOperation,
            IDeletePolicyOperation deletePolicyOperation,
            IGetPoliciesOperation getPoliciesOperation,
            IAddResourceToPolicyOperation addResourceToPolicyOperation,
            IDeleteResourceFromPolicyOperation deleteResourceFromPolicyOperation,
            IUpdatePolicyOperation updatePolicyOperation,
            IGetConfigurationOperation getConfigurationOperation,
            ISearchPoliciesOperation searchPoliciesOperation)
        {
            _addPolicyOperation = addPolicyOperation;
            _getPolicyOperation = getPolicyOperation;
            _deletePolicyOperation = deletePolicyOperation;
            _getPoliciesOperation = getPoliciesOperation;
            _addResourceToPolicyOperation = addResourceToPolicyOperation;
            _deleteResourceFromPolicyOperation = deleteResourceFromPolicyOperation;
            _updatePolicyOperation = updatePolicyOperation;
            _getConfigurationOperation = getConfigurationOperation;
            _searchPoliciesOperation = searchPoliciesOperation;
        }

        public Task<AddPolicyResult> Add(PostPolicy request, string url, string token)
        {
            return _addPolicyOperation.ExecuteAsync(request, url, token);
        }

        public async Task<AddPolicyResult> AddByResolution(PostPolicy request, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Add(request, policyEndpoint, token).ConfigureAwait(false);
        }

        public Task<GetPolicyResult> Get(string id, string url, string token)
        {
            return _getPolicyOperation.ExecuteAsync(id, url, token);
        }

        public async Task<GetPolicyResult> GetByResolution(string id, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Get(id, policyEndpoint, token).ConfigureAwait(false);
        }
        
        public Task<GetPoliciesResult> GetAll(string url, string token)
        {
            return _getPoliciesOperation.ExecuteAsync(url, token);
        }

        public async Task<GetPoliciesResult> GetAllByResolution(string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await GetAll(policyEndpoint, token).ConfigureAwait(false);
        }

        public Task<BaseResponse> Delete(string id, string url, string token)
        {
            return _deletePolicyOperation.ExecuteAsync(id, url, token);
        }

        public async Task<BaseResponse> DeleteByResolution(string id, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Delete(id, policyEndpoint, token).ConfigureAwait(false);
        }

        public Task<BaseResponse> Update(PutPolicy request, string url, string token)
        {
            return _updatePolicyOperation.ExecuteAsync(request, url, token);
        }

        public async Task<BaseResponse> UpdateByResolution(PutPolicy request, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Update(request, policyEndpoint, token).ConfigureAwait(false);
        }

        public Task<BaseResponse> AddResource(string id, PostAddResourceSet request, string url, string token)
        {
            return _addResourceToPolicyOperation.ExecuteAsync(id, request, url, token);
        }

        public async Task<BaseResponse> AddResourceByResolution(string id, PostAddResourceSet request, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await AddResource(id, request, policyEndpoint, token).ConfigureAwait(false);
        }

        public Task<BaseResponse> DeleteResource(string id, string resourceId, string url, string token)
        {
            return _deleteResourceFromPolicyOperation.ExecuteAsync(id, resourceId, url, token);
        }

        public async Task<BaseResponse> DeleteResourceByResolution(string id, string resourceId, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await DeleteResource(id, resourceId, policyEndpoint, token).ConfigureAwait(false);
        }

        public async Task<SearchAuthPoliciesResult> ResolveSearch(string url, SearchAuthPolicies parameter, string authorizationHeaderValue = null)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await _searchPoliciesOperation.ExecuteAsync(policyEndpoint + "/.search", parameter, authorizationHeaderValue).ConfigureAwait(false);
        }

        private async Task<string> GetPolicyEndPoint(Uri configurationUri)
        {
            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri).ConfigureAwait(false);
            return configuration.PoliciesEndpoint;
        }
    }
}
