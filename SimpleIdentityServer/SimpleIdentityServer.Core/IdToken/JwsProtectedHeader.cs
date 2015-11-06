namespace SimpleIdentityServer.Core.IdToken
{
    public class JwsProtectedHeader
    {
        /// <summary>
        /// Gets or sets the encoded object type. In general its value is JWT
        /// </summary>
        public string typ { get; set; }

        /// <summary>
        /// Gets or sets the algorithm used to secure the JwsProjtectedHeader & the JWS payload.
        /// </summary>
        public string alg { get; set; }
    }
}
