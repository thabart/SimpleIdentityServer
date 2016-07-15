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
using SimpleIdentityServer.Host.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.MiddleWare
{
    internal class AuthenticationMiddleware<TOptions> where TOptions : AuthenticationOptions, new()
    {
        private readonly List<string> _includedPaths = new List<string>
        {
            "/Authenticate",
            "/Authenticate/ExternalLogin",
            "/Authenticate/OpenId",
            "/Authenticate/LocalLoginOpenId",
            "/Authenticate/LocalLogin",
            "/Authenticate/ExternalLoginOpenId",
            "/signin-microsoft",
            "/signin-oidc",
            "/signin-facebook",
            "/signin-adfs",
            "/signin-google",
            "/signin-twitter"
        };

        private readonly RequestDelegate _next;

        private readonly AuthenticationOptions _options;

        private readonly IAuthenticationManager _authenticationManager;

        #region Constructor

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

            if (authenticationManager == null)
            {
                throw new ArgumentNullException(nameof(authenticationManager));
            }

            _next = next;
            _options = options.Value;
            _authenticationManager = authenticationManager;
        }

        #endregion

        #region Public methods

        public async Task Invoke(HttpContext context)
        {        
            if (_includedPaths.Contains(context.Request.Path) 
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

        #endregion
    }
}
