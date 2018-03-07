using SimpleIdentityServer.Manager.Client.Configuration;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Clients
{
    public interface IOpenIdClients
    {
        Task<IEnumerable<OpenIdClientResponse>> GetAll(Uri clientsUri, string authorizationHeaderValue = null);
        Task<IEnumerable<OpenIdClientResponse>> ResolveGetAll(Uri wellKnownConfigurationUri, string authorizationHeaderValue = null);
    }

    internal sealed class OpenIdClients : IOpenIdClients
    {
        private readonly IGetAllClientsOperation _getAllClientsOperation;
        private readonly IConfigurationClient _configurationClient;

        public OpenIdClients(IGetAllClientsOperation getAllClientsOperation, IConfigurationClient configurationClient)
        {
            _getAllClientsOperation = getAllClientsOperation;
            _configurationClient = configurationClient;
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
