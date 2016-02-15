using System.Threading.Tasks;

using SimpleIdentityServer.Client.DTOs.Request;
using SimpleIdentityServer.Client.DTOs.Response;
using SimpleIdentityServer.Client.Operations.Token;
using SimpleIdentityServer.Client.Parameters;

namespace SimpleIdentityServer.Client
{
    public interface IIdentityServerClient
    {
        Task<GrantedToken> GetAccessTokenViaResourceOwnerGrantTypeAsync(GetAccessToken getAccessToken);

        Task<GrantedToken> GetAccessTokenViaClientCredentialsGrantTypeAsync();
    }

    public class IdentityServerClient : IIdentityServerClient
    {
        private readonly IPostTokenOperation _postTokenOperation;

        public IdentityServerClient(IPostTokenOperation postTokenOperation)
        {
            _postTokenOperation = postTokenOperation;
        }

        public async Task<GrantedToken> GetAccessTokenViaResourceOwnerGrantTypeAsync(GetAccessToken getAccessToken)
        {
            var request = new TokenRequest
            {
                client_id = getAccessToken.ClientId,
                password = getAccessToken.Password,
                scope = getAccessToken.Scope,
                username = getAccessToken.Username,
                grant_type = GrantTypeRequest.password
            };
            return await _postTokenOperation.ExecuteAsync(request);
        }

        public async Task<GrantedToken> GetAccessTokenViaClientCredentialsGrantTypeAsync()
        {
            return null;
        }
    }
}
