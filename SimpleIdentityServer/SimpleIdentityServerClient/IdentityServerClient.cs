using System.Threading.Tasks;

using SimpleIdentityServerClient.DTOs.Request;
using SimpleIdentityServerClient.DTOs.Response;
using SimpleIdentityServerClient.Operations.Token;
using SimpleIdentityServerClient.Parameters;

namespace SimpleIdentityServerClient
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
