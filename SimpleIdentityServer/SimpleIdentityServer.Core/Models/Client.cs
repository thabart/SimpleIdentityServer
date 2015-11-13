using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Models
{
    public enum ResponseType
    {
        code,
        token,
        id_token
    }

    public enum GrantType
    {
        authorization_code,
        @implicit,
        refresh_token
    }

    public enum TokenEndPointAuthenticationMethods
    {
        // Defaut value
        client_secret_basic,
        client_secret_post,
        client_secret_jwt,
        private_key_jwt,
        none
    }

    public class Client
    {
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        public string ClientSecret { get; set; }

        public string DisplayName { get; set; }

        public string LogoUri { get; set; }

        /// <summary>
        /// Gets or sets the home page of the client.
        /// </summary>
        public string ClientUri { get; set; }

        /// <summary>
        /// Gets or sets the URL that the RP provides to the End-User to read about the how the profile data will be used.
        /// </summary>
        public string PolicyUri { get; set; }

        /// <summary>
        /// Gets or sets the URL that the RP provides to the End-User to read about the RP's terms of service.
        /// </summary>
        public string TosUri { get; set; }

        #region Encryption mechanism for ID TOKEN

        /// <summary>
        /// Gets or sets the JWS alg algorithm for signing the ID token issued to this client.
        /// The default is RS256. The public key for validating the signature is provided by retrieving the JWK Set referenced by the JWKS_URI
        /// </summary>
        public string IdTokenSignedTResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the JWE alg algorithm. REQUIRED for encrypting the ID token issued to this client.
        /// The default is that no encryption is performed
        /// </summary>
        public string IdTokenEncryptedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the JWE enc algorithm. REQUIRED for encrypting the ID token issued to this client.
        /// If IdTokenEncryptedResponseAlg is specified then the value is A128CBC-HS256
        /// </summary>
        public string IdTokenEncryptedResponseEnc { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the client authentication method for the Token Endpoint. 
        /// </summary>
        public TokenEndPointAuthenticationMethods TokenEndPointAuthMethod { get; set; }

        /// <summary>
        /// Gets or sets an array containing a list of OAUTH2.0 response_type values
        /// </summary>
        public List<ResponseType> ResponseTypes { get; set; }

        /// <summary>
        /// Gets or sets an array containing a list of OAUTH2.0 grant types
        /// </summary>
        public List<GrantType> GrantTypes { get; set; }

        /// <summary>
        /// Gets or sets a list of OAUTH2.0 grant_types.
        /// </summary>
        public List<Scope> AllowedScopes { get; set; }

        /// <summary>
        /// Gets or sets an array of Redirection URI values used by the client.
        /// </summary>
        public List<string> RedirectionUrls { get; set; }
    }
}
