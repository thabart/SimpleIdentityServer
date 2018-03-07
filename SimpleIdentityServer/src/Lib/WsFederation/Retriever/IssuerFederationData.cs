namespace WsFederation.Retriever
{
    /// <summary>
    /// Signing metadata parsed from a WSFed endpoint.
    /// </summary>
    
    public class IssuerFederationData
    {
        /// <summary>
        /// Gets or sets the passive token endpoint.
        /// </summary>
        public string PassiveTokenEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the token issuer name.
        /// </summary>
        public string TokenIssuerName { get; set; }
    }
}
