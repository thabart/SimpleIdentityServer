using SimpleIdentityServer.Core.Jwt;
using System.Collections.Generic;

namespace Parser
{
    public class FacebookClaimsParser
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
                "first_name",
                Constants.StandardResourceOwnerClaimNames.GivenName
            },
            {
                "last_name",
                Constants.StandardResourceOwnerClaimNames.FamilyName
            },
            {
                "gender",
                Constants.StandardResourceOwnerClaimNames.Gender
            },
            {
                "locale",
                Constants.StandardResourceOwnerClaimNames.Locale
            },
            {
                "picture",
                Constants.StandardResourceOwnerClaimNames.Picture
            },
            {
                "updated_at",
                Constants.StandardResourceOwnerClaimNames.UpdatedAt
            },
            {
                "email",
                Constants.StandardResourceOwnerClaimNames.Email
            },
            {
                "birthday",
                Constants.StandardResourceOwnerClaimNames.BirthDate
            },
            {
                "link",
                Constants.StandardResourceOwnerClaimNames.WebSite
            }
        };

        public Dictionary<string, object> Process(Dictionary<string, object> claims)
        {
            var result = new Dictionary<string, object>();
            foreach(var claim in claims)
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