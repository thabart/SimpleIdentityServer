using SimpleIdentityServer.Manager.Client;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Common.Responses;
using SimpleIdentityServer.ResourceManager.Core.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Stores;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.Scopes.Actions
{
    public interface IUpdateScopeAction
    {
        Task<BaseResponse> Execute(string subject, ScopeResponse request, EndpointTypes type);
    }

    internal sealed class UpdateScopeAction : IUpdateScopeAction
    {
        private readonly IOpenIdManagerClientFactory _openIdManagerClientFactory;
        private readonly IEndpointHelper _endpointHelper;
        private readonly ITokenStore _tokenStore;

        public UpdateScopeAction(IOpenIdManagerClientFactory openIdManagerClientFactory,
            IEndpointHelper endpointHelper, ITokenStore tokenStore)
        {
            _openIdManagerClientFactory = openIdManagerClientFactory;
            _endpointHelper = endpointHelper;
            _tokenStore = tokenStore;
        }

        public async Task<BaseResponse> Execute(string subject, ScopeResponse request, EndpointTypes type)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var endpoint = await _endpointHelper.TryGetEndpointFromProfile(subject, type);
            var grantedToken = await _tokenStore.GetToken(endpoint.AuthUrl, endpoint.ClientId, endpoint.ClientSecret, new[] { Constants.MANAGER_SCOPE });
            return await _openIdManagerClientFactory.GetScopesClient().ResolveUpdate(new Uri(endpoint.ManagerUrl), request, grantedToken.AccessToken);
        }
    }
}
