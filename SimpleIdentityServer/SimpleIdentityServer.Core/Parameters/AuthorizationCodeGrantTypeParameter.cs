namespace SimpleIdentityServer.Core.Parameters
{
    public class AuthorizationCodeGrantTypeParameter
    {
        /// <summary>
        /// Gets or sets the authorization code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the redirection url.
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the clients secret.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the client assertion type
        /// </summary>
        public string ClientAssertionType { get; set; }

        /// <summary>
        /// Gets or sets the client assertion.
        /// </summary>
        public string ClientAssertion { get; set; }
    }
}
