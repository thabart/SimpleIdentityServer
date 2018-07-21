﻿#region copyright
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
using Microsoft.Extensions.Primitives;
using SimpleIdentityServer.Uma.Common.DTOs;
using SimpleIdentityServer.Uma.Core.Api.IntrospectionController;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Introspection)]
    public class IntrospectionController : Controller
    {
        private readonly IIntrospectionActions _introspectionActions;

        public IntrospectionController(IIntrospectionActions introspectionActions)
        {
            _introspectionActions = introspectionActions;
        }
        
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return await Introspect().ConfigureAwait(false);
        }

        [HttpPost]
        public async Task<ActionResult> Post()
        {
            return await Introspect().ConfigureAwait(false);
        }

        [HttpPost("bulk")]
        public async Task<ActionResult> PostBulk([FromBody] PostIntrospection parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var result = await _introspectionActions.GetIntrospection(parameter.Rpts).ConfigureAwait(false);
            return new OkObjectResult(result);
        }

        private async Task<ActionResult> Introspect()
        {
            StringValues authorizationHeader;
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out authorizationHeader))
            {
                var authorizationHeaderValue = authorizationHeader.First();
                var splittedAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splittedAuthorizationHeaderValue.Count() == 2 &&
                    string.Equals(splittedAuthorizationHeaderValue.First(), "Bearer", StringComparison.CurrentCultureIgnoreCase))
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splittedAuthorizationHeaderValue[0],
                        splittedAuthorizationHeaderValue[1]);
                }
            }

            if (authenticationHeaderValue == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.Forbidden);
            }

            var result = await _introspectionActions.GetIntrospection(authenticationHeaderValue.Parameter).ConfigureAwait(false);
            return new OkObjectResult(result);
        }
    }
}
