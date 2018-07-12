using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccessToken.Store
{
    public interface IAccessTokenStore
    {
        Task<GrantedTokenResponse> GetToken(string url, string clientId, string clientSecret, IEnumerable<string> scopes);
    }
}