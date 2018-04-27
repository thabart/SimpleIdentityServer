using SimpleIdentityServer.Client;
using SimpleIdentityServer.ResourceManager.Core.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Stores;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.Resources.Actions
{
    public interface IGetAuthPoliciesByResourceAction
    {
        Task<SearchAuthPoliciesResponse> Execute(string url, string resourceId);
    }

    internal sealed class GetAuthPoliciesByResourceAction : IGetAuthPoliciesByResourceAction
    {
        private const string _scopeName = "uma_protection";
        private readonly IEndpointHelper _endpointHelper;
        private readonly IIdentityServerUmaClientFactory _identityServerUmaClientFactory;
        private readonly ITokenStore _tokenStore;

        public GetAuthPoliciesByResourceAction(IEndpointHelper endpointHelper, IIdentityServerUmaClientFactory identityServerUmaClientFactory, ITokenStore tokenStore)
        {
            _endpointHelper = endpointHelper;
            _identityServerUmaClientFactory = identityServerUmaClientFactory;
            _tokenStore = tokenStore;
        }

        public async Task<SearchAuthPoliciesResponse> Execute(string url, string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            var searchAuthPolicies = new SearchAuthPolicies
            {
                ResourceIds = new[] { resourceId },
                StartIndex = 0,
                TotalResults = 1000
            };
            var endpoint = await _endpointHelper.GetEndpoint(url, EndpointTypes.AUTH);
            var grantedToken = await _tokenStore.GetToken(endpoint.Url, endpoint.ClientId, endpoint.ClientSecret, new[] { _scopeName });
            return await _identityServerUmaClientFactory.GetPolicyClient().ResolveSearch(endpoint.Url, searchAuthPolicies, grantedToken.AccessToken);
        }
    }
}
