#region copyright
// Copyright 2016 Habart Thierry
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

namespace SimpleIdentityServer.Client.Scope
{
    public interface IScopeClient
    {
        Task<UpdateScopeResponse> Update(PutScope request, string url);
        Task<UpdateScopeResponse> UpdateByResolution(PutScope request, string url);
        Task<AddScopeResponse> Add(PostScope request, string url);
        Task<AddScopeResponse> AddByResolution(PostScope request, string url);
        Task<bool> Delete(string id, string url);
        Task<bool> DeleteByResolution(string id, string url);
        Task<IEnumerable<string>> GetAll(string url);
        Task<IEnumerable<string>> GetAllByResolution(string url);
        Task<ScopeResponse> Get(string id, string url);
        Task<ScopeResponse> GetByResolution(string id, string url);
    }

    internal class ScopeClient : IScopeClient
    {
        private readonly IGetScopeOperation _getScopeOperation;
        private readonly IGetScopesOperation _getScopesOperation;
        private readonly IDeleteScopeOperation _deleteScopeOperation;
        private readonly IUpdateScopeOperation _updateScopeOperation;
        private readonly IAddScopeOperation _addScopeOperation;
        private readonly IGetConfigurationOperation _getConfigurationOperation;

        public ScopeClient(IGetScopeOperation getScopeOperation,
            IGetScopesOperation getScopesOperation,
            IDeleteScopeOperation deleteScopeOperation,
            IUpdateScopeOperation updateScopeOperation,
            IAddScopeOperation addScopeOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _getScopeOperation = getScopeOperation;
            _getScopesOperation = getScopesOperation;
            _deleteScopeOperation = deleteScopeOperation;
            _updateScopeOperation = updateScopeOperation;
            _addScopeOperation = addScopeOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        public Task<UpdateScopeResponse> Update(PutScope request, string url)
        {
            return _updateScopeOperation.ExecuteAsync(request, url);
        }

        public async Task<UpdateScopeResponse> UpdateByResolution(PutScope request, string url)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await Update(request, configuration.ScopeEndPoint);
        }

        public Task<AddScopeResponse> Add(PostScope request, string url)
        {
            return _addScopeOperation.Execute(request, url);
        }

        public async Task<AddScopeResponse> AddByResolution(PostScope request, string url)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await Add(request, configuration.ScopeEndPoint);
        }

        public Task<bool> Delete(string id, string url)
        {
            return _deleteScopeOperation.ExecuteAsync(id, url);
        }

        public async Task<bool> DeleteByResolution(string id, string url)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await Delete(id, configuration.ScopeEndPoint);
        }

        public Task<IEnumerable<string>> GetAll(string url)
        {
            return _getScopesOperation.ExecuteAsync(url);
        }

        public async Task<IEnumerable<string>> GetAllByResolution(string url)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await GetAll(configuration.ScopeEndPoint);
        }

        public Task<ScopeResponse> Get(string id, string url)
        {
            return _getScopeOperation.Execute(id, url);
        }

        public async Task<ScopeResponse> GetByResolution(string id, string url)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url));
            return await Get(id, configuration.ScopeEndPoint);
        }
    }
}
