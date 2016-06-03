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

using Microsoft.AspNetCore.Authorization;
using SimpleIdentityServer.Uma.Common;
using System;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdentityServer.Uma.Authorization
{
    public class ConventionalUmaAuthorizationRequirement : AuthorizationHandler<ConventionalUmaAuthorizationRequirement>, IAuthorizationRequirement
    {
        #region Constructor

        public ConventionalUmaAuthorizationRequirement()
        {

        }

        #endregion

        #region Protected methods

        protected override void Handle(AuthorizationContext context, ConventionalUmaAuthorizationRequirement requirement)
        {
            var resource = context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext;
            if (context.User == null || resource == null)
            {
                return;
            }

            var claimsIdentity = context.User.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
            {
                return;
            }

            var routes = resource.RouteData.Values;
            object controller = null;
            object action = null;
            if (!routes.TryGetValue("action", out action) || !routes.TryGetValue("controller", out controller))
            {
                return;
            }

            var permissions = claimsIdentity.GetPermissions();
            if (permissions.Any(p => string.Equals(p.ApplicationName, controller.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(p.OperationName, action.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }
        }

        #endregion
    }
}
