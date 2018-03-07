using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Core.Jwt;
using System.Collections.Generic;
using System.Security.Claims;

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

        public List<Claim> Process(JObject claims)
        {
            var result = new List<Claim>();
            foreach (var claim in claims)
            {
                string key = claim.Key;
                if (_mappingLinkedinClaimsToOpenId.ContainsKey(claim.Key))
                {
                    key = _mappingLinkedinClaimsToOpenId[claim.Key];
                }

                result.Add(new Claim(key, claim.Value.ToString()));
            }

            return result;
        }
    }
}
