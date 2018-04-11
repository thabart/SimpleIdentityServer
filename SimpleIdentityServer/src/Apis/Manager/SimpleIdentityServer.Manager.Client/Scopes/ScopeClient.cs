using SimpleIdentityServer.Manager.Client.Configuration;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Scopes
{
    public interface IScopeClient
    {
        Task<BaseResponse> ResolveAdd(Uri wellKnownConfigurationUri, ScopeResponse scope, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolveUpdate(Uri wellKnownConfigurationUri, ScopeResponse client, string authorizationHeaderValue = null);
        Task<GetScopeResponse> ResolveGet(Uri wellKnownConfigurationUri, string scopeId, string authorizationHeaderValue = null);
        Task<BaseResponse> ResolvedDelete(Uri wellKnownConfigurationUri, string scopeId, string authorizationHeaderValue = null);
        Task<GetAllScopesResponse> GetAll(Uri scopesUri, string authorizationHeaderValue = null);
        Task<GetAllScopesResponse> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null);
    }

    internal sealed class ScopeClient : IScopeClient
    {
        private readonly IAddScopeOperation _addScopeOperation;
        private readonly IDeleteScopeOperation _deleteScopeOperation;
        private readonly IGetAllScopesOperation _getAllScopesOperation;
        private readonly IGetScopeOperation _getScopeOperation;
        private readonly IUpdateScopeOperation _updateScopeOperation;
        private readonly IConfigurationClient _configurationClient;

        public ScopeClient(IAddScopeOperation addScopeOperation, IDeleteScopeOperation deleteScopeOperation, IGetAllScopesOperation getAllScopesOperation, IGetScopeOperation getScopeOperation, 
            IUpdateScopeOperation updateScopeOperation, IConfigurationClient configurationClient)
        {
            _addScopeOperation = addScopeOperation;
            _deleteScopeOperation = deleteScopeOperation;
            _getAllScopesOperation = getAllScopesOperation;
            _getScopeOperation = getScopeOperation;
            _updateScopeOperation = updateScopeOperation;
            _configurationClient = configurationClient;
        }

        public async Task<BaseResponse> ResolveAdd(Uri wellKnownConfigurationUri, ScopeResponse scope, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _addScopeOperation.ExecuteAsync(new Uri(configuration.ScopesEndpoint), scope, authorizationHeaderValue);
        }

        public async Task<BaseResponse> ResolveUpdate(Uri wellKnownConfigurationUri, ScopeResponse client, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _updateScopeOperation.ExecuteAsync(new Uri(configuration.ScopesEndpoint), client, authorizationHeaderValue);
        }

        public async Task<GetScopeResponse> ResolveGet(Uri wellKnownConfigurationUri, string scopeId, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _getScopeOperation.ExecuteAsync(new Uri(configuration.ScopesEndpoint + "/" + scopeId), authorizationHeaderValue);
        }

        public async Task<BaseResponse> ResolvedDelete(Uri wellKnownConfigurationUri, string scopeId, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _deleteScopeOperation.ExecuteAsync(new Uri(configuration.ScopesEndpoint + "/" + scopeId), authorizationHeaderValue);
        }

        public Task<GetAllScopesResponse> GetAll(Uri scopesUri, string authorizationHeaderValue = null)
        {
            return _getAllScopesOperation.ExecuteAsync(scopesUri, authorizationHeaderValue);
        }

        public async Task<GetAllScopesResponse> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await GetAll(new Uri(configuration.ScopesEndpoint), authorizationHeaderValue);
        }
    }
}
