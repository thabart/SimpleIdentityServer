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

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.IdentityServer.EF.Models;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.IdentityServer.EF
{
    internal static class MappingExtensions
    {
        #region To domain

        public static Scope ToDomain(this IdentityServer4.EntityFramework.Entities.Scope scope)
        {
            var standardScopeNames = IdentityServer4.Models.StandardScopes.All.Select(s => s.Name);
            var record = new Scope
            {
                Name = scope.Name,
                Description = scope.Description,
                IsExposed = scope.ShowInDiscoveryDocument,
                IsDisplayedInConsent = true
            };
            record.IsOpenIdScope = standardScopeNames.Contains(record.Name);
            record.Type = scope.Type == (int)IdentityServer4.Models.ScopeType.Identity ?
                ScopeType.ResourceOwner : ScopeType.ProtectedApi;
            if (scope.Claims != null && scope.Claims.Any())
            {
                record.Claims = scope.Claims.Select(c => c.Name).ToList();
            }

            return record;
        }

        public static ResourceOwner ToDomain(this User user)
        {
            var result = new ResourceOwner
            {
                Id = user.Subject,
                Password = user.Password,
                Name = user.Username,
                IsLocalAccount = user.IsLocalAccount
            };

            if (user.Claims != null && user.Claims.Any())
            {
                result.BirthDate = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate);
                result.Email = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email);
                result.FamilyName = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName);
                result.Gender = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender);
                result.GivenName = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName);
                result.Locale = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale);
                result.MiddleName = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName);
                result.NickName = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName);
                result.Picture = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture);
                result.PhoneNumber = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber);
                result.PreferredUserName = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName);
                result.Profile = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile);
                result.WebSite = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite);
                result.ZoneInfo = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo);
                result.BirthDate = user.Claims.GetClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate);
                result.EmailVerified = user.Claims.GetClaimBool(Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified);
                result.PhoneNumberVerified = user.Claims.GetClaimBool(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified);
                result.Roles = user.Claims.GetClaims(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role);
            }

            return result;
        }

        #endregion

        #region Private static methods

        private static string GetClaim(this IEnumerable<Claim> claims, string key)
        {
            var claim = claims.FirstOrDefault(c => c.Key == key);
            if (claim == null)
            {
                return string.Empty;
            }

            return claim.Value;
        }

        private static bool GetClaimBool(this IEnumerable<Claim> claims, string key)
        {
            var claim = claims.FirstOrDefault(c => c.Key == key);
            if (claim == null)
            {
                return false;
            }

            bool result = false;
            if (!bool.TryParse(claim.Value, out result))
            {
                return result;
            }

            return result;
        }

        private static List<string> GetClaims(this IEnumerable<Claim> claims, string key)
        {
            return claims.Where(c => c.Key == key).Select(c => c.Value).ToList();
        }

        #endregion
    }
}
