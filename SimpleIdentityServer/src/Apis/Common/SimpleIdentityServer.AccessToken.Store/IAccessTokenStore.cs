using SimpleIdentityServer.Client.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccessToken.Store
{
    public interface IAccessTokenStore
    {
        Task<GrantedToken> GetToken(string url, string clientId, string clientSecret, IEnumerable<string> scopes);
    }
}