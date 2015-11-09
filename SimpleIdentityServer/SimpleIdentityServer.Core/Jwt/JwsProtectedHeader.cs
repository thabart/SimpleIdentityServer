namespace SimpleIdentityServer.Core.Jwt
{
    public class JwsProtectedHeader
    {
        /// <summary>
        /// Gets or sets the encoded object type. In general its value is JWT
        /// </summary>
        public string typ { get; set; }

        /// <summary>
        /// Gets or sets the algorithm used to secure the JwsProtectedHeader & the JWS payload.
        /// </summary>
        public string alg { get; set; }

        /// <summary>
        /// Gets or sets the identifier indicating the key that was used to secure the token.
        /// </summary>
        public string Kid { get; set; }
    }
}
