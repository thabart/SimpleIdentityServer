using SimpleIdentityServer.Client;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Host.Stores
{
    public interface IAccessTokenStore
    {
        Task<string> GetAccessToken();
    }

    internal sealed class AccessTokenStore : IAccessTokenStore
    {
        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        public AccessTokenStore(IIdentityServerClientFactory identityServerClientFactory)
        {
            _identityServerClientFactory = identityServerClientFactory;
        }

        public async Task<string> GetAccessToken()
        {
            var token = await _identityServerClientFactory.CreateAuthSelector()
                .UseClientSecretPostAuth("ResourceManagerApi", "ResourceManagerApi")
                .UseClientCredentials("display_configuration", "manage_configuration")
                .ResolveAsync("https://localhost:5443/.well-known/openid-configuration")
                .ConfigureAwait(false);
            return null;
        }
    }
}
