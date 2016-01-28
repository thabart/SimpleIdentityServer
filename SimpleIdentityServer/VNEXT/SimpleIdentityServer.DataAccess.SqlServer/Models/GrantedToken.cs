using System;

namespace SimpleIdentityServer.DataAccess.SqlServer.Models
{
    public class GrantedToken
    {
        public int Id { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string Scope { get; set; }

        public int ExpiresIn { get; set; }

        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the user information payload
        /// </summary>
        public string UserInfoPayLoad { get; set; }

        /// <summary>
        /// Gets or sets the identity token payload
        /// </summary>
        public string IdTokenPayLoad { get; set; }
    }
}
