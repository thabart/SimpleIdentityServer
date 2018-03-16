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

using System.Linq;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SimpleIdentityServer.Uma.Host.Extensions
{
    internal static class ControllerExtensions
    {
        public static AuthenticationHeaderValue GetAuthenticationHeader(this Controller controller)
        {
            const string authorizationName = "Authorization";
            StringValues values;
            if (!controller.Request.Headers.TryGetValue(authorizationName, out values))
            {
                return null;
            }

            var authorizationHeader = values.First();
            return AuthenticationHeaderValue.Parse(authorizationHeader);
        }

        public static string GetClientId(this Controller controller)
        {
            if (controller.User == null ||
                controller.User.Identity == null ||
                !controller.User.Identity.IsAuthenticated)
            {
                return string.Empty;
            }

            var claim = controller.User.Claims.FirstOrDefault(c => c.Type == "client_id");
            if (claim == null)
            {
                return string.Empty;
            }

            return claim.Value;
        }

        public static IEnumerable<Claim> GetClaims(this Controller controller)
        {
            if (controller.User == null ||
                controller.User.Identity == null ||
                !controller.User.Identity.IsAuthenticated)
            {
                return new List<Claim>();
            }

            return controller.User.Claims;
        }
    }
}
