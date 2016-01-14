using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.Fake.Models
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

    public enum ApplicationTypes
    {
        native,
        web
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

        public string ClientName { get; set; }

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
        public string IdTokenSignedResponseAlg { get; set; }

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

        /// <summary>
        /// Gets or sets the type of application
        /// </summary>
        public ApplicationTypes ApplicationType { get; set; }

        /// <summary>
        /// Url for the Client's JSON Web Key Set document
        /// </summary>
        public string JwksUri { get; set; }

        /// <summary>
        /// Gets or sets the list of json web keys
        /// </summary>
        public List<JsonWebKey> JsonWebKeys { get; set; }

        /// <summary>
        /// Gets or sets the list of contacts
        /// </summary>
        public List<string> Contacts { get; set; }

        /// <summary>
        /// Get or set the sector identifier uri
        /// </summary>
        public string SectorIdentifierUri { get; set; }

        /// <summary>
        /// Gets or sets the subject type
        /// </summary>
        public string SubjectType { get; set; }

        /// <summary>
        /// Gets or sets the user info signed response algorithm
        /// </summary>
        public string UserInfoSignedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the user info encrypted response algorithm
        /// </summary>
        public string UserInfoEncryptedResponseAlg { get; set; }

        /// <summary>
        /// Gets or sets the user info encrypted response enc
        /// </summary>
        public string UserInfoEncryptedResponseEnc { get; set; }

        /// <summary>
        /// Gets or sets the request objects signing algorithm
        /// </summary>
        public string RequestObjectSigningAlg { get; set; }

        /// <summary>
        /// Gets or sets the request object encryption algorithm
        /// </summary>
        public string RequestObjectEncryptionAlg { get; set; }

        /// <summary>
        /// Gets or sets the request object encryption enc
        /// </summary>
        public string RequestObjectEncryptionEnc { get; set; }

        /// <summary>
        /// Gets or sets the token endpoint authentication signing algorithm
        /// </summary>
        public string TokenEndPointAuthSigningAlg { get; set; }

        /// <summary>
        /// Gets or sets the default max age
        /// </summary>
        public double DefaultMaxAge { get; set; }

        /// <summary>
        /// Gets or sets the require authentication time
        /// </summary>
        public bool RequireAuthTime { get; set; }

        /// <summary>
        /// Gets or sets the default acr values
        /// </summary>
        public string DefaultAcrValues { get; set; }

        /// <summary>
        /// Gets or sets the initiate login uri
        /// </summary>
        public string InitiateLoginUri { get; set; }

        /// <summary>
        /// Gets or sets the list of request uris
        /// </summary>
        public List<string> RequestUris { get; set; } 
    }
}
