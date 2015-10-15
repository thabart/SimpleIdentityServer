using SimpleIdentityServerClient.DTOs;

using System.Threading.Tasks;

namespace SimpleIdentityServerClient
{
    public interface IIdentityServerClient
    {
        Task<GrantedToken> GetAccessTokenViaResourceOwnerGrantTypeAsync();

        Task<GrantedToken> GetAccessTokenViaClientCredentialsGrantTypeAsync();
    }

    public class IdentityServerClient : IIdentityServerClient
    {
        public async Task<GrantedToken> GetAccessTokenViaResourceOwnerGrantTypeAsync()
        {
            return null;
        }

        public async Task<GrantedToken> GetAccessTokenViaClientCredentialsGrantTypeAsync()
        {
            return null;
        }
    }
}
