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

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CustomerPortal
{
    public static class RazorPageExtensions
    {
        public static async Task<ClaimsPrincipal> GetAuthenticatedUser(this RazorPage razorPage)
        {
            if (razorPage == null)
            {
                throw new ArgumentNullException(nameof(razorPage));
            }

            var user = await razorPage.Context.Authentication.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return user ?? new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
