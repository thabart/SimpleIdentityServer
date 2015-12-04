using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface IGrantedTokenRepository
    {
        bool Insert(GrantedToken grantedToken);

        GrantedToken GetToken(string accessToken);

        GrantedToken GetToken(
            string scopes,
            string clientId,
            JwsPayload idTokenJwsPayload,
            JwsPayload userInfoJwsPayload);
    }
}
