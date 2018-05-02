using SimpleIdentityServer.Manager.Client;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.ResourceManager.Core.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Stores;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.ResourceOwners.Actions
{
    public interface ISearchResourceOwnersAction
    {
        Task<SearchResourceOwnerResponse> Execute(string subject, SearchResourceOwnersRequest request);
    }

    internal sealed class SearchResourceOwnersAction : ISearchResourceOwnersAction
    {
        private readonly IOpenIdManagerClientFactory _openIdManagerClientFactory;
        private readonly IEndpointHelper _endpointHelper;
        private readonly ITokenStore _tokenStore;

        public SearchResourceOwnersAction(IOpenIdManagerClientFactory openIdManagerClientFactory,
            IEndpointHelper endpointHelper, ITokenStore tokenStore)
        {
            _openIdManagerClientFactory = openIdManagerClientFactory;
            _endpointHelper = endpointHelper;
            _tokenStore = tokenStore;
        }

        public async Task<SearchResourceOwnerResponse> Execute(string subject, SearchResourceOwnersRequest request)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var endpoint = await _endpointHelper.TryGetEndpointFromProfile(subject, EndpointTypes.OPENID);
            var grantedToken = await _tokenStore.GetToken(endpoint.AuthUrl, endpoint.ClientId, endpoint.ClientSecret, new[] { Constants.MANAGER_SCOPE });
            return await _openIdManagerClientFactory.GetResourceOwnerClient().ResolveSearch(new Uri(endpoint.ManagerUrl), request, grantedToken.AccessToken);
        }
    }
}
