using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Models
{
    public enum ScopeType
    {
        ProtectedApi,
        ResourceOwner
    }

    public class Scope
    {
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a boolean whether the scope is displayed in the consent.
        /// </summary>
        public bool IsDisplayedInConsent { get; set; }

        /// <summary>
        /// Gets or sets a boolean whether the scope is internal : openid, profile ...
        /// </summary>
        public bool IsInternal { get; set; }

        /// <summary>
        /// Gets or sets a boolean whether the scope is exposed in the well-known configuration endpoint.
        /// </summary>
        public bool IsExposed { get; set; }

        public ScopeType Type { get; set; }

        public List<string> Claims { get; set; }
    }
}
