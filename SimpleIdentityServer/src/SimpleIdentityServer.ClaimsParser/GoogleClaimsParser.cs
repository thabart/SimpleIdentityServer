using Newtonsoft.Json.Linq;
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
                "name",
                Constants.StandardResourceOwnerClaimNames.Name
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

                var val = claim.Value as JObject;
                if (val != null)
                {
                    var t = val.GetType();
                    var children = val.First;
                }
                // result.Add(key, claim.Value);
            }

            return result;
        }
    }
}
