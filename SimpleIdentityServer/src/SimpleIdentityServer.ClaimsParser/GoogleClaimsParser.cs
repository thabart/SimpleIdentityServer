using SimpleIdentityServer.Core.Jwt;
using System.Collections.Generic;

namespace Parser
{
    public class GoogleClaimsParser
    {
        private readonly Dictionary<string, string> _mappingFacebookClaimToOpenId = new Dictionary<string, string>
        {
            {
                "id",
                Constants.StandardResourceOwnerClaimNames.Subject
            },
            {
                "displayName",
                Constants.StandardResourceOwnerClaimNames.Name
            },
            {
                "name.givenName",
                Constants.StandardResourceOwnerClaimNames.GivenName
            },
            {
                "name.familyName",
                Constants.StandardResourceOwnerClaimNames.FamilyName
            },
            {
                "url",
                Constants.StandardResourceOwnerClaimNames.WebSite
            },
            {
                "emails.value",
                Constants.StandardResourceOwnerClaimNames.Email
            }
        };

        public Dictionary<string, object> Process(Dictionary<string, object> claims)
        {
            var result = new Dictionary<string, object>();
            foreach (var claim in claims)
            {
                string key = claim.Key;
                foreach(var mapping in _mappingFacebookClaimToOpenId)
                {
                    var val = GetValue(claim, mapping);
                    if (val != null)
                    {
                        result.Add(mapping.Value, val);
                        break;
                    }
                }
            }

            return result;
        }

        private string GetValue(
            KeyValuePair<string, object> claim,
            KeyValuePair<string, string> mapping,
            int indice = 0)
        {
            var splitted = mapping.Key.Split('.');
            if (splitted.Length < 1 ||
                splitted.Length <= indice ||
                splitted[indice] != claim.Key)
            {
                return null;
            }

            var dic = claim.Value as Dictionary<string, object>;
            if (dic == null)
            {
                return claim.Value.ToString();
            }

            indice = indice + 1;
            foreach (var record in dic)
            {
                var val = GetValue(record, mapping, indice);
                if (val != null)
                {
                    return val;
                }
            }

            return null;         
        }
    }
}
