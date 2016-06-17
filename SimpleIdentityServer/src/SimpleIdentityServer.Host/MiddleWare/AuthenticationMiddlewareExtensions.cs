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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;

namespace SimpleIdentityServer.Host.MiddleWare
{
    internal static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthentication(
            this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            return applicationBuilder.UseMiddleware<AuthenticationMiddleware<AuthenticationOptions>>();
        }

        public static IApplicationBuilder UseAuthentication(
            this IApplicationBuilder applicationBuilder,
            AuthenticationOptions authenticationOptions)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            if (authenticationOptions == null)
            {
                throw new ArgumentNullException(nameof(authenticationOptions));
            }

            return applicationBuilder.UseMiddleware<AuthenticationMiddleware<AuthenticationOptions>>(Options.Create(authenticationOptions));
        }
    }
}
