using SimpleIdentityServer.Manager.Client;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.ResourceManager.Core.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Stores;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.Clients.Actions
{
    public interface IGetClientAction
    {
        Task<GetClientResponse> Execute(string subject, string clientId, EndpointTypes type);
    }

    internal sealed class GetClientAction : IGetClientAction
    {
        private readonly IOpenIdManagerClientFactory _openIdManagerClientFactory;
        private readonly IEndpointHelper _endpointHelper;
        private readonly ITokenStore _tokenStore;

        public GetClientAction(IOpenIdManagerClientFactory openIdManagerClientFactory,
            IEndpointHelper endpointHelper, ITokenStore tokenStore)
        {
            _openIdManagerClientFactory = openIdManagerClientFactory;
            _endpointHelper = endpointHelper;
            _tokenStore = tokenStore;
        }

        public async Task<GetClientResponse> Execute(string subject, string clientId, EndpointTypes type)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var endpoint = await _endpointHelper.TryGetEndpointFromProfile(subject, type);
            var grantedToken = await _tokenStore.GetToken(endpoint.AuthUrl, endpoint.ClientId, endpoint.ClientSecret, new[] { Constants.MANAGER_SCOPE });
            return await _openIdManagerClientFactory.GetOpenIdsClient().ResolveGet(new Uri(endpoint.ManagerUrl), clientId, grantedToken.AccessToken);
        }
    }
}
