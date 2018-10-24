using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;

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
            { ClaimTypes.Locality, Constants.StandardResourceOwnerClaimNames.Locale },
            { ClaimTypes.Role, Constants.StandardResourceOwnerClaimNames.Role }
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

        public static IEnumerable<Claim> ToClaims(this IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var result = new List<Claim>();
            foreach (var claim in claims)
            {
                if (_mappingToOpenidClaims.ContainsValue(claim.Type))
                {
                    var kvp = _mappingToOpenidClaims.First(m => m.Value == claim.Type);
                    result.Add(new Claim(kvp.Key, claim.Value));
                }
                else
                {
                    result.Add(claim);
                }
            }

            return result;
        }
		
		public static object GetClaimValue(this Claim claim)
        {
            try
            {
                return JsonConvert.DeserializeObject(claim.Value);
            }
            catch { }
			
             return claim.Value;
        }
    }
}
