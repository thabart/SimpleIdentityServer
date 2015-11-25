using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.Jwt.Mapping
{
    public interface IClaimsMapping
    {
        Dictionary<string, string> MapToOpenIdClaims(IEnumerable<Claim> claims);
    }

    public class ClaimsMapping : IClaimsMapping
    {
        public Dictionary<string, string> MapToOpenIdClaims(IEnumerable<Claim> claims)
        {
            var result = new Dictionary<string, string>();
            foreach (var claim in claims)
            {
                if (Constants.MapWifClaimsToOpenIdClaims.ContainsKey(claim.Type))
                {
                    result.Add(Constants.MapWifClaimsToOpenIdClaims[claim.Type], claim.Value);
                }
                else
                {
                    result.Add(claim.Type, claim.Value);
                }
            }

            return result;
        }
    }
}
