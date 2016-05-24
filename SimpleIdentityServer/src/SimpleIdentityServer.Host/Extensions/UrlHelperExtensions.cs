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
using Microsoft.AspNetCore.Mvc;

namespace SimpleIdentityServer.Host.Extensions
{
    public static class UriHelperExtensions 
    {
        private static IHttpContextAccessor HttpContextAccessor;
        
        public static void Configure(IHttpContextAccessor httpContextAccessor) 
        {
            HttpContextAccessor = httpContextAccessor;
        }
        
        public static string AbsoluteAction(this IUrlHelper urlHelper, string actionName, string controllerName, object routeValues = null) 
        {
            var scheme  = HttpContextAccessor.HttpContext.Request.Scheme;
            return urlHelper.Action(actionName, controllerName, routeValues, scheme);
        }
    }
}