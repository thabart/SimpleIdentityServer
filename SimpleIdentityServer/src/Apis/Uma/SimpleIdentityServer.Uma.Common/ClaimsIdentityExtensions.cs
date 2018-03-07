#region copyright
// Copyright 2016 Habart Thierry
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

using Newtonsoft.Json;
using SimpleIdentityServer.Uma.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdentityServer.Uma.Common
{
    public class Permission
    {
        public string ResourceSetId { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public string Url { get; set; }
    }

    public static class ClaimsIdentityExtensions
    {
        private const string PermissionName = "permission";

        public static void AddPermission(this ClaimsIdentity claimsIdentity, Permission permission)
        {
            if (claimsIdentity == null)
            {
                throw new ArgumentNullException(nameof(claimsIdentity));
            }

            if (permission == null)
            {
                throw new ArgumentNullException(nameof(permission));
            }

            var json = JsonConvert.SerializeObject(permission);
            claimsIdentity.AddClaim(new Claim(PermissionName, json));
        }

        public static IEnumerable<Permission> GetPermissions(this ClaimsIdentity claimsIdentity)
        {
            if (claimsIdentity == null)
            {
                throw new ArgumentNullException(nameof(claimsIdentity));
            }

            var result = new List<Permission>();
            var claims = claimsIdentity.Claims.Where(c => c.Type == PermissionName).ToList();
            foreach (var claim in claims)
            {
                result.Add(JsonConvert.DeserializeObject<Permission>(claim.Value));
            }

            return result;
        }
    }
}
