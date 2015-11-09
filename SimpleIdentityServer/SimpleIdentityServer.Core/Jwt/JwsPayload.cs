using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.Jwt
{
    /// <summary>
    /// Represents a JSON Web Token
    /// </summary>
    public class JwsPayload
    {
        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        public string iss { get; set; }

        /// <summary>
        /// Gets or sets the audience(s)
        /// </summary>
        public string[] aud { get; set; }

        /// <summary>
        /// Gets or sets the expiration time
        /// </summary>
        public double exp { get; set; }

        /// <summary>
        /// Gets or sets the IAT
        /// </summary>
        public double iat { get; set; }

        /// <summary>
        /// Gets or sets the authentication time
        /// </summary>
        public double auth_time { get; set; }

        /// <summary>
        /// Gets or sets the NONCE
        /// </summary>
        public string nonce { get; set; }

        /// <summary>
        /// Gets or sets the authentication context class reference
        /// </summary>
        public string acr { get; set; }

        /// <summary>
        /// Gets or sets the Authentication Methods References
        /// </summary>
        public string amr { get; set; }

        /// <summary>
        /// Gets or sets the Authorized party
        /// </summary>
        public string azp { get; set; }

        /// <summary>
        /// Gets or sets the claims
        /// </summary>
        public List<Claim> Claims { get; set; }
    }
}
