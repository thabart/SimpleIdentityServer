using System.Collections.Generic;

namespace SimpleIdentityServer.Authenticate.Basic
{
    public class BasicAuthenticationOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthorizationWellKnownConfiguration { get; set; }
    }

    public class BasicAuthenticateOptions
    {
        public BasicAuthenticateOptions()
        {
            IsScimResourceAutomaticallyCreated = false;
            ClaimsIncludedInUserCreation = new List<string>();
        }

        /// <summary>
        /// List of claims include when the resource owner is created.
        /// If the list is empty then all the claims are included.
        /// </summary>
        public IEnumerable<string> ClaimsIncludedInUserCreation { get; set; }
        /// <summary>
        /// Base url of the SCIM server.
        /// </summary>
        public string ScimBaseUrl { get; set; }
        /// <summary>
        /// Credentials used to get an access token.
        /// </summary>
        public BasicAuthenticationOptions AuthenticationOptions { get; set; }
        /// <summary>
        /// Enable / Disable the automatic creation of SCIM resource.
        /// </summary>
        public bool IsScimResourceAutomaticallyCreated { get; set; }
    }
}
