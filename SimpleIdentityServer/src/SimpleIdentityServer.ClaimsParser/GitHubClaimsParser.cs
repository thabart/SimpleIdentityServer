using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Core.Jwt;
using System.Collections.Generic;
using System.Security.Claims;

namespace Parser
{
    public class GitHubClaimsParser
    {
        private readonly Dictionary<string, string> _mappingFacebookClaimToOpenId = new Dictionary<string, string>
        {
            {
                "id",
                Constants.StandardResourceOwnerClaimNames.Subject
            },
            {
                "name",
                Constants.StandardResourceOwnerClaimNames.Name
            },
            {
                "avatar_url",
                Constants.StandardResourceOwnerClaimNames.Picture
            },
            {
                "updated_at",
                Constants.StandardResourceOwnerClaimNames.UpdatedAt
            },
            {
                "email",
                Constants.StandardResourceOwnerClaimNames.Email
            }
        };

        public List<Claim> Process(JObject jObj)
        {
            var result = new List<Claim>();
            foreach (var claim in jObj)
            {
                string key = claim.Key;
                if (_mappingFacebookClaimToOpenId.ContainsKey(claim.Key))
                {
                    key = _mappingFacebookClaimToOpenId[claim.Key];
                }

                result.Add(new Claim(key, claim.Value.ToString()));
            }

            return result;
        }
    }
}
