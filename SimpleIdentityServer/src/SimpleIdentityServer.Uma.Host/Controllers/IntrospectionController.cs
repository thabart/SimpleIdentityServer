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
using Microsoft.Extensions.Primitives;
using SimpleIdentityServer.Core.Api.Introspection;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common.Serializers;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Linq;
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

        [HttpPost]
        public async Task<Introspection> Post()
        {
            if (Request.Form == null)
            {
                throw new ArgumentNullException(nameof(Request.Form));
            }

            var serializer = new ParamSerializer();
            var introspectionRequest = serializer.Deserialize<IntrospectionRequest>(Request.Form);
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

            var result = await _introspectionActions.PostIntrospection(introspectionRequest.ToParameter(), authenticationHeaderValue);
            return result.ToDto();
        }
    }
}
