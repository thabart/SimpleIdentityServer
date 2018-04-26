using SimpleIdentityServer.Client;
using SimpleIdentityServer.ResourceManager.Core.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Stores;
using SimpleIdentityServer.Uma.Common.DTOs;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Resources.Actions
{
    public interface ISearchResourcesetAction
    {
        Task<SearchResourceSetResponse> Execute(string url, SearchResourceSet searchResourceSet);
    }

    internal class SearchResourcesetAction : ISearchResourcesetAction
    {
        private const string _scopeName = "uma_protection";
        private readonly IEndpointHelper _endpointHelper;
        private readonly IIdentityServerUmaClientFactory _identityServerUmaClientFactory;
        private readonly ITokenStore _tokenStore;

        public SearchResourcesetAction(IEndpointHelper endpointHelper, IIdentityServerUmaClientFactory identityServerUmaClientFactory, ITokenStore tokenStore)
        {
            _endpointHelper = endpointHelper;
            _identityServerUmaClientFactory = identityServerUmaClientFactory;
            _tokenStore = tokenStore;
        }

        public async Task<SearchResourceSetResponse> Execute(string url, SearchResourceSet searchResourceSet)
        {
            var endpoint = await _endpointHelper.GetEndpoint(url, EndpointTypes.AUTH);
            var grantedToken = await _tokenStore.GetToken(endpoint.Url, endpoint.ClientId, endpoint.ClientSecret, new[] { _scopeName });
            return await _identityServerUmaClientFactory.GetResourceSetClient().ResolveSearch(endpoint.Url, searchResourceSet, grantedToken.AccessToken);
        }
    }
}
