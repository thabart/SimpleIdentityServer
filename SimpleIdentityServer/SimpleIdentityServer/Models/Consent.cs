using System.Collections.Generic;

namespace SimpleIdentityServer.Api.Models
{
    public class Consent
    {
        public string ClientDisplayName { get; set; }

        public List<string> AllowedScopeDescriptions { get; set; } 
    }
}