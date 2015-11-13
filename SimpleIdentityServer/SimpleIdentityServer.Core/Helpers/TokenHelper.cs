using System;
using SimpleIdentityServer.Core.Models;
using System.Text;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface ITokenHelper
    {
        GrantedToken GenerateToken(string scope);
    }

    public class TokenHelper : ITokenHelper
    {
        public GrantedToken GenerateToken(string scope)
        {
            var accessTokenId = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            var refreshTokenId = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            return new GrantedToken
            {
                AccessToken = Convert.ToBase64String(accessTokenId),
                ExpiresIn = 3600,
                Scope = scope,
                TokenType = Constants.StandardTokenTypes.Bearer,
                RefreshToken = Convert.ToBase64String(refreshTokenId)
            };
        }
    }
}
