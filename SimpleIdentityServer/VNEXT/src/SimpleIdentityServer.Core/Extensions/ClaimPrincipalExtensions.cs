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

using System.Security.Claims;

namespace SimpleIdentityServer.Core.Extensions
{
    public static class ClaimPrincipalExtensions
    {
        /// <summary>
        /// Returns if the user is authenticated
        /// </summary>
        /// <param name="principal">The user principal</param>
        /// <returns>The user is authenticated</returns>
        public static bool IsAuthenticated(this ClaimsPrincipal principal)
        {
            if (principal == null ||
                principal.Identity == null)
            {
                return false;
            }

            return principal.Identity.IsAuthenticated;
        }

        /// <summary>
        /// Returns the subject from an authenticated user
        /// Otherwise returns null.
        /// </summary>
        /// <param name="principal">The user principal</param>
        /// <returns>User's subject</returns>
        public static string GetSubject(this ClaimsPrincipal principal)
        {
            if (principal == null ||
                principal.Identity == null)
            {
                return null;
            }

            var claim = principal.FindFirst(Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
            if (claim == null)
            {
                claim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null)
                {
                    return null;
                }
            }

            return claim.Value;
        }
    }
}
