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

using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;

namespace SimpleIdentityServer.Uma.Host.Extensions
{
    internal static class ControllerExtensions
    {
        #region Public static methods

        public static AuthenticationHeaderValue GetAuthenticationHeader(this Controller controller)
        {
            const string authorizationName = "Authorization";
            StringValues values;
            if (!controller.Request.Headers.TryGetValue(authorizationName, out values))
            {
                return null;
            }

            var authorizationHeader = values.First();
            return AuthenticationHeaderValue.Parse(authorizationHeader);
        }

        #endregion
    }
}
