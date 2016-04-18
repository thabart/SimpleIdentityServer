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

using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Primitives;
using SimpleIdentityServer.Core.Api.Introspection;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.DTOs.Response;
using SimpleIdentityServer.Host.Extensions;
using System;
using System.Linq;
using System.Net.Http.Headers;

namespace SimpleIdentityServer.Host.Controllers.Api
{
    [Route(Constants.EndPoints.Introspection)]
    public class IntrospectionController : Controller
    {
        private readonly IIntrospectionActions _introspectionActions;

        #region Constructor

        public IntrospectionController(IIntrospectionActions introspectionActions)
        {
            _introspectionActions = introspectionActions;
        }

        #endregion

        #region Public methods

        [HttpPost]
        public IntrospectionResponse Post([FromForm] IntrospectionRequest introspectionRequest)
        {
            if (introspectionRequest == null)
            {
                throw new ArgumentNullException(nameof(introspectionRequest));
            }

            StringValues authorizationHeader;
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out authorizationHeader))
            {
                var authorizationHeaderValue = authorizationHeader.First();
                var splittedAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splittedAuthorizationHeaderValue.Count() == 2)
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splittedAuthorizationHeaderValue[0],
                        splittedAuthorizationHeaderValue[1]);
                }
            }

            var result = _introspectionActions.PostIntrospection(
                introspectionRequest.ToParameter(), 
                authenticationHeaderValue);
            return result.ToDto();
        }

        #endregion
    }
}
