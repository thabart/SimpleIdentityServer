#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
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

        public static List<string> GetAllClaimsNames(this ClaimsParameter parameter)
        {
            var result = new List<string>();
            if (!parameter.IsAnyUserInfoClaimParameter() &&
                !parameter.IsAnyUserInfoClaimParameter())
            {
                return result;
            }

            if (!parameter.IsAnyUserInfoClaimParameter())
            {
                result.AddRange(parameter.UserInfo.Select(s => s.Name));
            }

            if (!parameter.IsAnyIdentityTokenClaimParameter())
            {
                result.AddRange(parameter.IdToken.Select(s => s.Name));
            }

            return result;
        } 

        private static bool IsStandardClaim(string claimName)
        {
            return Jwt.Constants.AllStandardResourceOwnerClaimNames.Contains(claimName) ||
                Jwt.Constants.AllStandardClaimNames.Contains(claimName);
        }
    }
}
