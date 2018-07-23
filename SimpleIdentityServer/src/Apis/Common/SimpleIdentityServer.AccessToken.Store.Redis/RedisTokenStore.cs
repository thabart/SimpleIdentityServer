using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccessToken.Store.Redis
{
    internal sealed class RedisTokenStore : IAccessTokenStore
    {
        public Task<GrantedTokenResponse> GetToken(string url, string clientId, string clientSecret, IEnumerable<string> scopes)
        {
            // TODO : IMPLEMENT.
            return null;
        }
    }
}
