using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Jwt
{
    [DataContract]
    public class JwsProtectedHeader
    {
        /// <summary>
        /// Gets or sets the encoded object type. In general its value is JWT
        /// </summary>
        [DataMember(Name = "typ")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the algorithm used to secure the JwsProtectedHeader & the JWS payload.
        /// </summary>
        [DataMember(Name = "alg")]
        public string Alg { get; set; }

        /// <summary>
        /// Gets or sets the identifier indicating the key that was used to secure the token.
        /// </summary>
        [DataMember(Name = "kid")]
        public string Kid { get; set; }
    }
}
