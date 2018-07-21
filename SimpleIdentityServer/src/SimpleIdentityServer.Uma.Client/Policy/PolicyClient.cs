﻿#region copyright
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
        Task<PolicyResponse> Get(string id, string url, string token);
        Task<PolicyResponse> GetByResolution(string id, string url, string token);
        Task<IEnumerable<string>> GetAll(string url, string token);
        Task<IEnumerable<string>> GetAllByResolution(string url, string token);
        Task<bool> Delete(string id, string url, string token);
        Task<bool> DeleteByResolution(string id, string url, string token);
        Task<bool> Update(PutPolicy request, string url, string token);
        Task<bool> UpdateByResolution(PutPolicy request, string url, string token);
        Task<bool> AddResource(string id, PostAddResourceSet request, string url, string token);
        Task<bool> AddResourceByResolution(string id, PostAddResourceSet request, string url, string token);
        Task<bool> DeleteResource(string id, string resourceId, string url, string token);
        Task<bool> DeleteResourceByResolution(string id, string resourceId, string url, string token);
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

        public PolicyClient(
            IAddPolicyOperation addPolicyOperation,
            IGetPolicyOperation getPolicyOperation,
            IDeletePolicyOperation deletePolicyOperation,
            IGetPoliciesOperation getPoliciesOperation,
            IAddResourceToPolicyOperation addResourceToPolicyOperation,
            IDeleteResourceFromPolicyOperation deleteResourceFromPolicyOperation,
            IUpdatePolicyOperation updatePolicyOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _addPolicyOperation = addPolicyOperation;
            _getPolicyOperation = getPolicyOperation;
            _deletePolicyOperation = deletePolicyOperation;
            _getPoliciesOperation = getPoliciesOperation;
            _addResourceToPolicyOperation = addResourceToPolicyOperation;
            _deleteResourceFromPolicyOperation = deleteResourceFromPolicyOperation;
            _updatePolicyOperation = updatePolicyOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        public Task<AddPolicyResponse> Add(PostPolicy request, string url, string token)
        {
            return _addPolicyOperation.ExecuteAsync(request, url, token);
        }

        public async Task<AddPolicyResponse> AddByResolution(PostPolicy request, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Add(request, policyEndpoint, token).ConfigureAwait(false);
        }

        public Task<PolicyResponse> Get(string id, string url, string token)
        {
            return _getPolicyOperation.ExecuteAsync(id, url, token);
        }

        public async Task<PolicyResponse> GetByResolution(string id, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Get(id, policyEndpoint, token).ConfigureAwait(false);
        }
        
        public Task<IEnumerable<string>> GetAll(string url, string token)
        {
            return _getPoliciesOperation.ExecuteAsync(url, token);
        }

        public async Task<IEnumerable<string>> GetAllByResolution(string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await GetAll(policyEndpoint, token).ConfigureAwait(false);
        }

        public Task<bool> Delete(string id, string url, string token)
        {
            return _deletePolicyOperation.ExecuteAsync(id, url, token);
        }

        public async Task<bool> DeleteByResolution(string id, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Delete(id, policyEndpoint, token).ConfigureAwait(false);
        }

        public Task<bool> Update(PutPolicy request, string url, string token)
        {
            return _updatePolicyOperation.ExecuteAsync(request, url, token);
        }

        public async Task<bool> UpdateByResolution(PutPolicy request, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Update(request, policyEndpoint, token).ConfigureAwait(false);
        }

        public Task<bool> AddResource(string id, PostAddResourceSet request, string url, string token)
        {
            return _addResourceToPolicyOperation.ExecuteAsync(id, request, url, token);
        }

        public async Task<bool> AddResourceByResolution(string id, PostAddResourceSet request, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await AddResource(id, request, policyEndpoint, token).ConfigureAwait(false);
        }

        public Task<bool> DeleteResource(string id, string resourceId, string url, string token)
        {
            return _deleteResourceFromPolicyOperation.ExecuteAsync(id, resourceId, url, token);
        }

        public async Task<bool> DeleteResourceByResolution(string id, string resourceId, string url, string token)
        {
            var policyEndpoint = await GetPolicyEndPoint(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await DeleteResource(id, resourceId, policyEndpoint, token).ConfigureAwait(false);
        }
        
        private async Task<string> GetPolicyEndPoint(Uri configurationUri)
        {
            if (configurationUri == null)
            {
                throw new ArgumentNullException(nameof(configurationUri));
            }

            var configuration = await _getConfigurationOperation.ExecuteAsync(configurationUri).ConfigureAwait(false);
            return configuration.PolicyEndPoint;
        }
    }
}
