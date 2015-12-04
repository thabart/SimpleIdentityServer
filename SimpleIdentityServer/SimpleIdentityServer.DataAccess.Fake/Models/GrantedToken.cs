using SimpleIdentityServer.Core.Jwt;
using System;

namespace SimpleIdentityServer.DataAccess.Fake.Models
{
    public class GrantedToken
    {
        public string AccessToken { get; set; }

        public string IdToken { get; set; }

        public string TokenType { get; set; }

        public string RefreshToken { get; set; }

        public int ExpiresIn { get; set; }

        public string Scope { get; set; }

        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the user information payload
        /// </summary>
        public JwsPayload UserInfoPayLoad { get; set; }

        /// <summary>
        /// Gets or sets the identity token payload
        /// </summary>
        public JwsPayload IdTokenPayLoad { get; set; }
    }
}
