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

    public class Client
    {
        public string ClientId { get; set; }

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
