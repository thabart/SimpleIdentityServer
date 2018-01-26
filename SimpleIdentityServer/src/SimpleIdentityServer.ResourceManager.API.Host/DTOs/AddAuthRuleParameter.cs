using System.Collections.Generic;

namespace SimpleIdentityServer.ResourceManager.API.Host.DTOs
{
    internal sealed class AddAuthRuleClaimParameter
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    internal sealed class AddAuthRuleParameter
    {
        public string RuleId { get; set; }
        public IEnumerable<string> OpenIdClients { get; set; }
        public IEnumerable<string> OpenIdScopes { get; set; }
        public IEnumerable<AddAuthRuleClaimParameter> Claims { get; set; }
    }
}
