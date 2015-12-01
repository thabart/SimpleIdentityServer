using SimpleIdentityServer.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Core.Extensions
{
    public static class ClaimsParameterExtensions
    {
        /// <summary>
        /// Gets all the standard claim names from the ClaimsParameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static List<string> GetClaimNames(this ClaimsParameter parameter)
        {
            var result = new List<string>();

            var fillInClaims = new Action<List<ClaimParameter>, List<string>>((cps, r) =>
            {
                if (cps != null &&
                    cps.Any())
                {
                    r.AddRange(cps
                        .Where(cl => IsStandardClaim(cl.Name))
                        .Select(s => s.Name));
                }
            });

            fillInClaims(parameter.IdToken, result);
            fillInClaims(parameter.UserInfo, result);
            return result;
        }

        /// <summary>
        /// Return a boolean which indicates if the ClaimsParameter contains at least one user-info claim parameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static bool IsAnyUserInfoClaimParameter(this ClaimsParameter parameter)
        {
            return parameter != null &&
                parameter.UserInfo != null &&
                parameter.UserInfo.Any();
        }

        /// <summary>
        /// Returns a boolean which indicates if the ClaimsParameter contains at least one identity-token claim parameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static bool IsAnyIdentityTokenClaimParameter(this ClaimsParameter parameter)
        {
            return parameter != null &&
                parameter.IdToken != null &&
                parameter.IdToken.Any();
        }

        private static bool IsStandardClaim(string claimName)
        {
            return Jwt.Constants.AllStandardResourceOwnerClaimNames.Contains(claimName) ||
                Jwt.Constants.AllStandardClaimNames.Contains(claimName);
        }
    }
}
