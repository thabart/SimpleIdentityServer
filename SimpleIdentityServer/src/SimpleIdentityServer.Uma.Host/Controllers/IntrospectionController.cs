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
using SimpleIdentityServer.Uma.Core.Api.IntrospectionController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Introspection)]
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

        [HttpGet]
        public ActionResult Get()
        {
            return Introspect();
        }

        [HttpPost]
        public ActionResult Post()
        {
            return Introspect();
        }

        #endregion

        #region Private methods

        private ActionResult Introspect()
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
                return new HttpStatusCodeResult((int)HttpStatusCode.Forbidden);
            }

            var result = _introspectionActions.GetIntrospection(authenticationHeaderValue.Parameter);
            return new HttpOkObjectResult(result);
        }

        #endregion
    }
}
