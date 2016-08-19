using SimpleIdentityServer.Core.Jwt;
using System.Collections.Generic;

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

        public Dictionary<string, object> Process(Dictionary<string, object> claims)
        {
            var result = new Dictionary<string, object>();
            foreach (var claim in claims)
            {
                string key = claim.Key;
                if (_mappingFacebookClaimToOpenId.ContainsKey(claim.Key))
                {
                    key = _mappingFacebookClaimToOpenId[claim.Key];
                }

                result.Add(key, claim.Value);
            }

            return result;
        }
    }
}
