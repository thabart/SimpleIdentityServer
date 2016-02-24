using System.Collections.Generic;

namespace SimpleIdentityServer.DataAccess.SqlServer.Models
{
    public class Claim
    {
        public string Code { get; set; }

        public List<Scope> Scopes { get; set; }

        /// <summary>
        /// Gets or sets the list of consents
        /// </summary>
        public List<Consent> Consents { get; set; } 
    }
}
