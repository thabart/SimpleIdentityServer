using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.Jwt.Extensions
{
    public static class ClaimsExtensions
    {
        private static Dictionary<string, string> _mappingToOpenidClaims = new Dictionary<string, string>
        {
            { ClaimTypes.NameIdentifier, Constants.StandardResourceOwnerClaimNames.Subject },
            { ClaimTypes.DateOfBirth, Constants.StandardResourceOwnerClaimNames.BirthDate },
            { ClaimTypes.Email, Constants.StandardResourceOwnerClaimNames.Email },
            { ClaimTypes.Name, Constants.StandardResourceOwnerClaimNames.Name },
            { ClaimTypes.GivenName, Constants.StandardResourceOwnerClaimNames.GivenName },
            { ClaimTypes.Surname, Constants.StandardResourceOwnerClaimNames.FamilyName },
            { ClaimTypes.Gender, Constants.StandardResourceOwnerClaimNames.Gender },
            { ClaimTypes.Locality, Constants.StandardResourceOwnerClaimNames.Locale }
        };

        public static IEnumerable<Claim> ToOpenidClaims(this IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var result = new List<Claim>();
            foreach(var claim in claims)
            {
                if (_mappingToOpenidClaims.ContainsKey(claim.Type))
                {
                    result.Add(new Claim(_mappingToOpenidClaims[claim.Type], claim.Value));
                }
                else
                {
                    result.Add(claim);
                }
            }

            return result;
        }
    }
}
