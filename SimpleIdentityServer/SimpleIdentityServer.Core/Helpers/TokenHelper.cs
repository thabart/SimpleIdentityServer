using System;
using SimpleIdentityServer.Core.Models;
using System.Text;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface ITokenHelper
    {
        GrantedToken GenerateToken(
            string scope, 
            string idToken);
    }

    public class TokenHelper : ITokenHelper
    {
        public GrantedToken GenerateToken(
            string scope, 
            string idToken)
        {
            var accessTokenId = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            var refreshTokenId = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            return new GrantedToken
            {
                AccessToken = Convert.ToBase64String(accessTokenId),
                RefreshToken = Convert.ToBase64String(refreshTokenId),
                IdToken = idToken,
                ExpiresIn = 3600,
                Scope = scope,
                TokenType = Constants.StandardTokenTypes.Bearer,
                CreateDateTime = DateTime.UtcNow
            };
        }
    }
}
