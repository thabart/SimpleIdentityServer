using SimpleIdentityServer.Manager.Client;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.ResourceManager.Core.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Stores;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.ResourceOwners.Actions
{
    public interface IDeleteResourceOwnerAction
    {
        Task<BaseResponse> Execute(string subject, string resourceOwnerId);
    }

    internal sealed class DeleteResourceOwnerAction : IDeleteResourceOwnerAction
    {
        private readonly IOpenIdManagerClientFactory _openIdManagerClientFactory;
        private readonly IEndpointHelper _endpointHelper;
        private readonly ITokenStore _tokenStore;

        public DeleteResourceOwnerAction(IOpenIdManagerClientFactory openIdManagerClientFactory,
            IEndpointHelper endpointHelper, ITokenStore tokenStore)
        {
            _openIdManagerClientFactory = openIdManagerClientFactory;
            _endpointHelper = endpointHelper;
            _tokenStore = tokenStore;
        }

        public async Task<BaseResponse> Execute(string subject, string resourceOwnerId)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(resourceOwnerId))
            {
                throw new ArgumentNullException(nameof(resourceOwnerId));
            }

            var endpoint = await _endpointHelper.TryGetEndpointFromProfile(subject, EndpointTypes.OPENID);
            var grantedToken = await _tokenStore.GetToken(endpoint.AuthUrl, endpoint.ClientId, endpoint.ClientSecret, new[] { Constants.MANAGER_SCOPE });
            return await _openIdManagerClientFactory.GetResourceOwnerClient().ResolvedDelete(new Uri(endpoint.ManagerUrl), resourceOwnerId, grantedToken.AccessToken);
        }
    }
}
