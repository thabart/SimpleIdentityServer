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

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.Extensions
{
    public static class ClaimPrincipalExtensions
    {
        #region Public static methods

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

            return principal.Claims.GetSubject();            
        }

        public static string GetSubject(this IEnumerable<Claim> claims)
        {
            var claim = GetSubjectClaim(claims);
            return claim?.Value;
        }

        public static Claim GetSubjectClaim(this IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                return null;
            }

            var claim = claims.FirstOrDefault(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
            if (claim == null)
            {
                claim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (claim == null)
                {
                    return null;
                }
            }

            return claim;
        }

        public static string GetName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.Name);
        }

        public static string GetEmail(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.Email);
        }

        public static bool GetEmailVerified(this ClaimsPrincipal principal)
        {
            return GetBooleanClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified);
        }

        public static string GetFamilyName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName);
        }

        public static string GetGender(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.Gender);
        }

        public static string GetGivenName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.GivenName);
        }

        public static string GetLocale(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.Locale);
        }

        public static string GetMiddleName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName);
        }

        public static string GetNickName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.NickName);
        }

        public static string GetPhoneNumber(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber);
        }

        public static bool GetPhoneNumberVerified(this ClaimsPrincipal principal)
        {
            return GetBooleanClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified);
        }

        public static string GetPicture(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.Picture);
        }

        public static string GetPreferredUserName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName);
        }

        public static string GetProfile(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.Profile);
        }

        public static string GetRole(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.Role);
        }

        public static string GetWebSite(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.WebSite);
        }

        public static string GetZoneInfo(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo);
        }

        public static string GetBirthDate(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate);
        }

        #endregion

        #region Private static methods

        private static string GetClaimValue(ClaimsPrincipal principal, string claimName)
        {
            if (principal == null ||
                principal.Identity == null)
            {
                return null;
            }

            var claim = principal.FindFirst(claimName);
            if (claim == null)
            {
                return null;
            }

            return claim.Value;
        }

        private static bool GetBooleanClaimValue(ClaimsPrincipal principal, string claimName)
        {
            var result = GetClaimValue(principal, claimName);
            if (string.IsNullOrWhiteSpace(result))
            {
                return false;
            }

            bool res = false;
            if (!bool.TryParse(result, out res))
            {
                return false;
            }

            return true;
        }

        #endregion 
    }
}
