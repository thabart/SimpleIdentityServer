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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authentication.Middleware
{
    public class AuthenticationMiddleware<TOptions> where TOptions : AuthenticationMiddlewareOptions, new()
    {
        private readonly RequestDelegate _next;

        private readonly AuthenticationMiddlewareOptions _options;

        private readonly IAuthenticationManager _authenticationManager;

        public AuthenticationMiddleware(
            RequestDelegate next,
            IOptions<TOptions> options,
            IAuthenticationManager authenticationManager)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Value.ConfigurationEdp == null)
            {
                throw new ArgumentNullException(nameof(options.Value.ConfigurationEdp));
            }

            if (options.Value.IdServer == null)
            {
                throw new ArgumentNullException(nameof(options.Value.IdServer));
            }

            if (authenticationManager == null)
            {
                throw new ArgumentNullException(nameof(authenticationManager));
            }

            _next = next;
            _options = options.Value;
            _authenticationManager = authenticationManager;
        }

        public async Task Invoke(HttpContext context)
        {
            if ((context.Request.Path.HasValue && _options.IdServer.LoginUrls.Any(x => x.Equals(context.Request.Path.Value, StringComparison.OrdinalIgnoreCase))
                || context.Request.Path.Value.StartsWith("/signin-"))
                && (context.User == null ||
                    context.User.Identity == null ||
                    !context.User.Identity.IsAuthenticated))
            {
                if (!await _authenticationManager.Initialize(context, _options))
                {
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
