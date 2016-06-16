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
using SimpleIdentityServer.Host.Handlers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.MiddleWare
{
    internal class AuthenticationMiddleware
    {
        private readonly List<string> _includedPaths = new List<string>
        {
            "/Authenticate",
            "/Authenticate/ExternalLogin",
            "/signin-microsoft"
        };

        private readonly RequestDelegate _next;

        private readonly ExceptionHandlerMiddlewareOptions _options;

        private readonly IAuthenticationManager _authenticationManager;

        #region Constructor

        public AuthenticationMiddleware(
            RequestDelegate next, 
            IAuthenticationManager authenticationManager)
        {
            _next = next;
            _authenticationManager = authenticationManager;
        }

        #endregion

        #region Public methods

        public async Task Invoke(HttpContext context)
        {        
            if (_includedPaths.Contains(context.Request.Path))
            {
                if (!await _authenticationManager.Initialize(context))
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
