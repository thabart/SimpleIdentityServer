using System.Collections.Generic;

namespace SimpleIdentityServer.EF.Models
{
    public class Claim
    {
        public string Code { get; set; }
        public string Type { get; set; }
        public bool IsIdentifier { get; set; }
        public virtual List<ScopeClaim> ScopeClaims { get; set; }
        /// <summary>
        /// Gets or sets the list of consents
        /// </summary>
        public virtual List<ConsentClaim> ConsentClaims { get; set; }
        public virtual List<ResourceOwnerClaim> ResourceOwnerClaims { get; set; }
    }
}
