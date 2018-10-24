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
using SimpleIdentityServer.Uma.Client.ResourceSet;
using SimpleIdentityServer.Uma.Client.Results;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.ResourceSet
{
    public interface IResourceSetClient
    {
        Task<UpdateResourceSetResult> Update(PutResourceSet request, string url, string token);
        Task<UpdateResourceSetResult> UpdateByResolution(PutResourceSet request, string url, string token);
        Task<AddResourceSetResult> Add(PostResourceSet request, string url, string token);
        Task<AddResourceSetResult> AddByResolution(PostResourceSet request, string url, string token);
        Task<BaseResponse> Delete(string id, string url, string token);
        Task<BaseResponse> DeleteByResolution(string id, string url, string token);
        Task<GetResourcesResult> GetAll(string url, string token);
        Task<GetResourcesResult> GetAllByResolution(string url, string token);
        Task<GetResourceSetResult> Get(string id,  string url, string token);
        Task<GetResourceSetResult> GetByResolution(string id, string url, string token);
        Task<SearchResourceSetResult> ResolveSearch(string url, SearchResourceSet parameter, string authorizationHeaderValue = null);
    }

    internal class ResourceSetClient : IResourceSetClient
    {
        private readonly IAddResourceSetOperation _addResourceSetOperation;
        private readonly IDeleteResourceSetOperation _deleteResourceSetOperation;
        private readonly IGetResourcesOperation _getResourcesOperation;
        private readonly IGetResourceOperation _getResourceOperation;
        private readonly IUpdateResourceOperation _updateResourceOperation;
        private readonly IGetConfigurationOperation _getConfigurationOperation;
        private readonly ISearchResourcesOperation _searchResourcesOperation;

        public ResourceSetClient(
            IAddResourceSetOperation addResourceSetOperation,
            IDeleteResourceSetOperation deleteResourceSetOperation,
            IGetResourcesOperation getResourcesOperation,
            IGetResourceOperation getResourceOperation,
            IUpdateResourceOperation updateResourceOperation,
            IGetConfigurationOperation getConfigurationOperation,
            ISearchResourcesOperation searchResourcesOperation)
        {
            _addResourceSetOperation = addResourceSetOperation;
            _deleteResourceSetOperation = deleteResourceSetOperation;
            _getResourcesOperation = getResourcesOperation;
            _getResourceOperation = getResourceOperation;
            _updateResourceOperation = updateResourceOperation;
            _getConfigurationOperation = getConfigurationOperation;
            _searchResourcesOperation = searchResourcesOperation;
        }

        public Task<UpdateResourceSetResult> Update(PutResourceSet request, string url, string token)
        {
            return _updateResourceOperation.ExecuteAsync(request, url, token);
        }

        public async Task<UpdateResourceSetResult> UpdateByResolution(PutResourceSet request, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Update(request, configuration.ResourceRegistrationEndpoint, token).ConfigureAwait(false);
        }

        public Task<AddResourceSetResult> Add(PostResourceSet request, string url, string token)
        {
            return _addResourceSetOperation.ExecuteAsync(request, url, token);
        }

        public async Task<AddResourceSetResult> AddByResolution(PostResourceSet request, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Add(request, configuration.ResourceRegistrationEndpoint, token).ConfigureAwait(false);
        }

        public Task<BaseResponse> Delete(string id, string url, string token)
        {
            return _deleteResourceSetOperation.ExecuteAsync(id, url, token);
        }

        public async Task<BaseResponse> DeleteByResolution(string id, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Delete(id, configuration.ResourceRegistrationEndpoint, token).ConfigureAwait(false);
        }

        public Task<GetResourcesResult> GetAll(string url, string token)
        {
            return _getResourcesOperation.ExecuteAsync(url, token);
        }

        public async Task<GetResourcesResult> GetAllByResolution(string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await GetAll(configuration.ResourceRegistrationEndpoint, token).ConfigureAwait(false);
        }

        public Task<GetResourceSetResult> Get(string id, string url, string token)
        {
            return _getResourceOperation.ExecuteAsync(id, url, token);
        }

        public async Task<GetResourceSetResult> GetByResolution(string id, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Get(id, configuration.ResourceRegistrationEndpoint, token).ConfigureAwait(false);
        }

        public async Task<SearchResourceSetResult> ResolveSearch(string url, SearchResourceSet parameter, string authorizationHeaderValue = null)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await _searchResourcesOperation.ExecuteAsync(configuration.ResourceRegistrationEndpoint + "/.search", parameter, authorizationHeaderValue).ConfigureAwait(false);
        }
    }
}
