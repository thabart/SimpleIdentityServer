using System;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using System.Text;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IGrantedTokenGeneratorHelper
    {
        GrantedToken GenerateToken(
            string clientId,
            string scope,
            JwsPayload userInformationPayload = null,
            JwsPayload idTokenPayload = null);
    }

    // TODO : rename to granted token factory
    public class GrantedTokenGeneratorHelper : IGrantedTokenGeneratorHelper
    {
        public GrantedToken GenerateToken(
            string clientId,
            string scope,
            JwsPayload userInformationPayload = null,
            JwsPayload idTokenPayload = null)
        {
            var accessTokenId = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            var refreshTokenId = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            return new GrantedToken
            {
                AccessToken = Convert.ToBase64String(accessTokenId),
                RefreshToken = Convert.ToBase64String(refreshTokenId),
                ExpiresIn = 3600,
                TokenType = Constants.StandardTokenTypes.Bearer,
                CreateDateTime = DateTime.UtcNow,
                // IDS
                Scope = scope,
                UserInfoPayLoad = userInformationPayload,
                IdTokenPayLoad = idTokenPayload,
                ClientId = clientId
            };
        }
    }
}
