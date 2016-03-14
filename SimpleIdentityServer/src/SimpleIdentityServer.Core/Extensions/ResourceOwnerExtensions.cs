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

using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.Extensions
{
    public static class ResourceOwnerExtensions
    {
        public static List<Claim> ToClaims(this ResourceOwner resourceOwner)
        {
            var claims = new List<Claim>();
            if (resourceOwner == null)
            {
                return claims;
            }

            if (!string.IsNullOrEmpty(resourceOwner.Id))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, resourceOwner.Id));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.BirthDate))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate, resourceOwner.BirthDate));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.Email))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Email, resourceOwner.Email));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.FamilyName))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName, resourceOwner.FamilyName));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.Gender))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Gender, resourceOwner.Gender));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.GivenName))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.GivenName, resourceOwner.GivenName));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.Locale))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Locale, resourceOwner.Locale));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.MiddleName))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName, resourceOwner.MiddleName));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.Name))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Name, resourceOwner.Name));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.NickName))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.NickName, resourceOwner.NickName));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.PhoneNumber))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, resourceOwner.PhoneNumber));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.Picture))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Picture, resourceOwner.Picture));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.PreferredUserName))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName, resourceOwner.PreferredUserName));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.Profile))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Profile, resourceOwner.Profile));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.WebSite))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.WebSite, resourceOwner.WebSite));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.ZoneInfo))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo, resourceOwner.ZoneInfo));
            }

            var address = resourceOwner.Address;
            if (address != null)
            {
                var serializedAddress = address.SerializeWithDataContract();
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Address, serializedAddress));
            }

            if (resourceOwner.Roles != null && resourceOwner.Roles.Any())
            {
                resourceOwner.Roles.ForEach(r => claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Role, r)));
            }

            claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified, resourceOwner.EmailVerified.ToString()));
            claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified, resourceOwner.PhoneNumberVerified.ToString()));
            claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, resourceOwner.UpdatedAt.ToString(CultureInfo.InvariantCulture)));

            return claims;
        } 
    }
}
