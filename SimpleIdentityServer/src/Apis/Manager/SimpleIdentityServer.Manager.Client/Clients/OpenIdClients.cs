using SimpleIdentityServer.Manager.Client.Configuration;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Clients
{
    public interface IOpenIdClients
    {
        Task<OpenIdClientResponse> ResolveGet(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null);
        Task<bool> ResolvedDelete(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null);
        Task<IEnumerable<OpenIdClientResponse>> GetAll(Uri clientsUri, string authorizationHeaderValue = null);
        Task<IEnumerable<OpenIdClientResponse>> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null);
    }

    internal sealed class OpenIdClients : IOpenIdClients
    {
        private readonly IGetAllClientsOperation _getAllClientsOperation;
        private readonly IDeleteClientOperation _deleteClientOperation;
        private readonly IGetClientOperation _getClientOperation;
        private readonly IConfigurationClient _configurationClient;

        public OpenIdClients(IGetAllClientsOperation getAllClientsOperation, IDeleteClientOperation deleteClientOperation,
            IGetClientOperation getClientOperation, IConfigurationClient configurationClient)
        {
            _getAllClientsOperation = getAllClientsOperation;
            _deleteClientOperation = deleteClientOperation;
            _getClientOperation = getClientOperation;
            _configurationClient = configurationClient;
        }

        public async Task<OpenIdClientResponse> ResolveGet(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _getClientOperation.ExecuteAsync(new Uri(configuration.ClientsEndpoint + "/" + clientId), authorizationHeaderValue);
        }

        public async Task<bool> ResolvedDelete(Uri wellKnownConfigurationUri, string clientId, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await _deleteClientOperation.ExecuteAsync(new Uri(configuration.ClientsEndpoint + "/" + clientId), authorizationHeaderValue);
        }

        public Task<IEnumerable<OpenIdClientResponse>> GetAll(Uri clientsUri, string authorizationHeaderValue = null)
        {
            return _getAllClientsOperation.ExecuteAsync(clientsUri, authorizationHeaderValue);
        }

        public async Task<IEnumerable<OpenIdClientResponse>> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null)
        {
            var configuration = await _configurationClient.GetConfiguration(wellKnownConfigurationUri);
            return await GetAll(new Uri(configuration.ClientsEndpoint), authorizationHeaderValue);
        }
    }
}
