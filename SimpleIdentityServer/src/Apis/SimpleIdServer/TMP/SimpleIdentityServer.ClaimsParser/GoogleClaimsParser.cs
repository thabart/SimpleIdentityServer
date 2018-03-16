using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Core.Jwt;
using System.Collections.Generic;
using System.Security.Claims;

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

        public List<Claim> Process(JObject jObj)
        {
            var result = new List<Claim>();
            foreach (var claim in jObj)
            {
                string key = claim.Key;
                foreach(var mapping in _mappingFacebookClaimToOpenId)
                {
                    var val = GetValue(claim, mapping);
                    if (val != null)
                    {
                        result.Add(new Claim(mapping.Value, val));
                        break;
                    }
                }
            }

            return result;
        }

        private string GetValue(
            KeyValuePair<string, JToken> claim,
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

            var dic = claim.Value as JObject;
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
