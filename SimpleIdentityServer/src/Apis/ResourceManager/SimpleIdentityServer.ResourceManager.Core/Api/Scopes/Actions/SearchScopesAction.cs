using SimpleIdentityServer.Manager.Client;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.ResourceManager.Core.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Stores;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.Scopes.Actions
{
    public interface ISearchScopesAction
    {
        Task<SearchScopeResponse> Execute(string subject, SearchScopesRequest searchScopesRequest, EndpointTypes type);
    }

    internal sealed class SearchScopesAction : ISearchScopesAction
    {
        private readonly IOpenIdManagerClientFactory _openIdManagerClientFactory;
        private readonly IEndpointHelper _endpointHelper;
        private readonly ITokenStore _tokenStore;

        public SearchScopesAction(IOpenIdManagerClientFactory openIdManagerClientFactory,
            IEndpointHelper endpointHelper, ITokenStore tokenStore)
        {
            _openIdManagerClientFactory = openIdManagerClientFactory;
            _endpointHelper = endpointHelper;
            _tokenStore = tokenStore;
        }

        public async Task<SearchScopeResponse> Execute(string subject, SearchScopesRequest searchScopesRequest, EndpointTypes type)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (searchScopesRequest == null)
            {
                throw new ArgumentNullException(nameof(searchScopesRequest));
            }

            var endpoint = await _endpointHelper.TryGetEndpointFromProfile(subject, type);
            var grantedToken = await _tokenStore.GetToken(endpoint.AuthUrl, endpoint.ClientId, endpoint.ClientSecret, new[] { Constants.MANAGER_SCOPE });
            return await _openIdManagerClientFactory.GetScopesClient().ResolveSearch(new Uri(endpoint.ManagerUrl), searchScopesRequest, grantedToken.AccessToken);
        }
    }
}
