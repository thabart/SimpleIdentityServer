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
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.ResourceSet
{
    public interface IResourceSetClient
    {
        Task<UpdateResourceSetResponse> Update(PutResourceSet request, string url, string token);
        Task<UpdateResourceSetResponse> UpdateByResolution(PutResourceSet request, string url, string token);
        Task<AddResourceSetResponse> Add(PostResourceSet request, string url, string token);
        Task<AddResourceSetResponse> AddByResolution(PostResourceSet request, string url, string token);
        Task<bool> Delete(string id, string url, string token);
        Task<bool> DeleteByResolution(string id, string url, string token);
        Task<IEnumerable<string>> GetAll(string url, string token);
        Task<IEnumerable<string>> GetAllByResolution(string url, string token);
        Task<ResourceSetResponse> Get(string id,  string url, string token);
        Task<ResourceSetResponse> GetByResolution(string id, string url, string token);
    }

    internal class ResourceSetClient : IResourceSetClient
    {
        private readonly IAddResourceSetOperation _addResourceSetOperation;
        private readonly IDeleteResourceSetOperation _deleteResourceSetOperation;
        private readonly IGetResourcesOperation _getResourcesOperation;
        private readonly IGetResourceOperation _getResourceOperation;
        private readonly IUpdateResourceOperation _updateResourceOperation;
        private readonly IGetConfigurationOperation _getConfigurationOperation;

        public ResourceSetClient(
            IAddResourceSetOperation addResourceSetOperation,
            IDeleteResourceSetOperation deleteResourceSetOperation,
            IGetResourcesOperation getResourcesOperation,
            IGetResourceOperation getResourceOperation,
            IUpdateResourceOperation updateResourceOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _addResourceSetOperation = addResourceSetOperation;
            _deleteResourceSetOperation = deleteResourceSetOperation;
            _getResourcesOperation = getResourcesOperation;
            _getResourceOperation = getResourceOperation;
            _updateResourceOperation = updateResourceOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        public Task<UpdateResourceSetResponse> Update(PutResourceSet request, string url, string token)
        {
            return _updateResourceOperation.ExecuteAsync(request, url, token);
        }

        public async Task<UpdateResourceSetResponse> UpdateByResolution(PutResourceSet request, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await Update(request, configuration.ResourceSetRegistrationEndPoint, token);
        }

        public Task<AddResourceSetResponse> Add(PostResourceSet request, string url, string token)
        {
            return _addResourceSetOperation.ExecuteAsync(request, url, token);
        }

        public async Task<AddResourceSetResponse> AddByResolution(PostResourceSet request, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await Add(request, configuration.ResourceSetRegistrationEndPoint, token);
        }

        public async Task<bool> Delete(string id, string url, string token)
        {
            return await _deleteResourceSetOperation.ExecuteAsync(id, url, token);
        }

        public async Task<bool> DeleteByResolution(string id, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await Delete(id, configuration.ResourceSetRegistrationEndPoint, token);
        }

        public Task<IEnumerable<string>> GetAll(string url, string token)
        {
            return _getResourcesOperation.ExecuteAsync(url, token);
        }

        public async Task<IEnumerable<string>> GetAllByResolution(string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await GetAll(configuration.ResourceSetRegistrationEndPoint, token);
        }

        public Task<ResourceSetResponse> Get(string id, string url, string token)
        {
            return _getResourceOperation.ExecuteAsync(id, url, token);
        }

        public async Task<ResourceSetResponse> GetByResolution(string id, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await Get(id, configuration.ResourceSetRegistrationEndPoint, token);
        }
    }
}
