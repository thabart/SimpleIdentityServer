using SimpleIdentityServer.Core.Jwt;
using System.Collections.Generic;

namespace Parser
{
    public class LinkedinClaimsParser
    {
        private readonly Dictionary<string, string> _mappingLinkedinClaimsToOpenId = new Dictionary<string, string>
        {
            {
                "id",
                Constants.StandardResourceOwnerClaimNames.Subject
            },
            {
                "firstName",
                Constants.StandardResourceOwnerClaimNames.GivenName
            },
            {
                "lastName",
                Constants.StandardResourceOwnerClaimNames.FamilyName
            }
        };

        public Dictionary<string, object> Process(Dictionary<string, object> claims)
        {
            var result = new Dictionary<string, object>();
            foreach (var claim in claims)
            {
                string key = claim.Key;
                if (_mappingLinkedinClaimsToOpenId.ContainsKey(claim.Key))
                {
                    key = _mappingLinkedinClaimsToOpenId[claim.Key];
                }

                result.Add(key, claim.Value);
            }

            return result;
        }
    }
}
