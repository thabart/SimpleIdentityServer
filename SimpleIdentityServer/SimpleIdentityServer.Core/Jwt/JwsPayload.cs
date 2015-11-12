using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Jwt
{
    /// <summary>
    /// Represents a JSON Web Token
    /// </summary>
    [DataContract]
    public class JwsPayload
    {
        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        [DataMember(Name = "iss")]
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the audience(s)
        /// </summary>
        [DataMember(Name = "aud")]
        public string[] Audiences { get; set; }

        /// <summary>
        /// Gets or sets the expiration time
        /// </summary>
        [DataMember(Name = "exp")]
        public double ExpirationTime { get; set; }

        /// <summary>
        /// Gets or sets the IAT
        /// </summary>
        [DataMember(Name = "iat")]
        public double Iat { get; set; }

        /// <summary>
        /// Gets or sets the authentication time
        /// </summary>
        [DataMember(Name = "auth_time")]
        public double AuthenticationTime { get; set; }

        /// <summary>
        /// Gets or sets the NONCE
        /// </summary>
        [DataMember(Name = "nonce")]
        public string Nonce { get; set; }

        /// <summary>
        /// Gets or sets the authentication context class reference
        /// </summary>
        [DataMember(Name = "acr")]
        public string Acr { get; set; }

        /// <summary>
        /// Gets or sets the Authentication Methods References
        /// </summary>
        [DataMember(Name = "amr")]
        public string Amr { get; set; }

        /// <summary>
        /// Gets or sets the Authorized party
        /// </summary>
        [DataMember(Name = "azp")]
        public string Azp { get; set; }

        /// <summary>
        /// Gets or sets the claims
        /// </summary>
        [DataMember(Name = "claims")]
        public Dictionary<string, string> Claims { get; set; }

        public string GetClaimValue(string claimName)
        {
            if (!Claims.ContainsKey(claimName))
            {
                return null;
            }

            return Claims[claimName];
        }
    }
}
