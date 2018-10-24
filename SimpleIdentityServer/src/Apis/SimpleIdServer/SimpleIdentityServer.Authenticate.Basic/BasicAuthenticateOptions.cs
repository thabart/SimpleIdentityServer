using System.Collections.Generic;

namespace SimpleIdentityServer.Authenticate.Basic
{
    public class BasicAuthenticateOptions
    {
        public BasicAuthenticateOptions()
        {
            ClaimsIncludedInUserCreation = new List<string>();
        }

        /// <summary>
        /// List of claims include when the resource owner is created.
        /// If the list is empty then all the claims are included.
        /// </summary>
        public IEnumerable<string> ClaimsIncludedInUserCreation { get; set; }
    }
}
