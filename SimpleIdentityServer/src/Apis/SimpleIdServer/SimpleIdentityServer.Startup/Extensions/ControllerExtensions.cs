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

using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Startup.Extensions
{
    internal static class ControllerExtensions
    {
        public static async Task<ClaimsPrincipal> GetAuthenticatedUserExternal(this Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            var user = await controller.HttpContext.Authentication.AuthenticateAsync(Constants.ExternalCookieName);
            return user ?? new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
