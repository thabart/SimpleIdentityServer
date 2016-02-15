using System.Collections.Generic;

namespace SimpleIdentityServer.Host.ViewModels
{
    public class ConsentViewModel
    {
        public string ClientDisplayName { get; set; }

        public List<string> AllowedScopeDescriptions { get; set; }

        public List<string> AllowedIndividualClaims { get; set; }

        public string LogoUri { get; set; }

        public string PolicyUri { get; set; }

        public string TosUri { get; set; }

        public string Code { get; set; }
    }
}