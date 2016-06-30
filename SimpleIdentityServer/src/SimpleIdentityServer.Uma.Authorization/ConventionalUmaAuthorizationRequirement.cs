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
using Microsoft.AspNetCore.Mvc.Controllers;
using SimpleIdentityServer.Uma.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

namespace SimpleIdentityServer.Uma.Authorization
{
    public class ConventionalUmaAuthorizationRequirement : AuthorizationHandler<ConventionalUmaAuthorizationRequirement>, IAuthorizationRequirement
    {
        private readonly ConventionalUmaOptions _conventionalUmaOptions;

        #region Constructor

        public ConventionalUmaAuthorizationRequirement(ConventionalUmaOptions conventionalUmaOptions)
        {
            _conventionalUmaOptions = conventionalUmaOptions;
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

            var controllerActionDescriptor = resource.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null)
            {
                return;
            }

            var projectName = controllerActionDescriptor.ControllerTypeInfo.Assembly.GetName().Name;
            var version = "v1";
            var controllerName = controllerActionDescriptor.ControllerTypeInfo.Name;
            var actionName = GetActionName(controllerActionDescriptor.MethodInfo);
            if (_conventionalUmaOptions != null)
            {
                if (!string.IsNullOrWhiteSpace(_conventionalUmaOptions.ProductName))
                {
                    projectName = _conventionalUmaOptions.ProductName;
                }

                if (!string.IsNullOrWhiteSpace(_conventionalUmaOptions.Version))
                {
                    version = _conventionalUmaOptions.Version;
                }
            }

            var expectedUrl = $"resources/Apis/{projectName}/{version}/{controllerName}/{actionName}";
            var permissions = claimsIdentity.GetPermissions();
            if (permissions.Any(p => string.Equals(expectedUrl, p.Url, StringComparison.CurrentCultureIgnoreCase) 
                && p.Scopes.Any(s => s == "execute")))
            {
                context.Succeed(requirement);
            }
        }

        private static string GetActionName(MethodInfo methodInfo)
        {
            var actionName = methodInfo.Name;
            var parameterNames = new List<string>();
            foreach(var parameter in  methodInfo.GetParameters())
            {
                parameterNames.Add(parameter.Name);
            }

            if (parameterNames.Any())
            {
                actionName = actionName + "{" + string.Join(",", parameterNames) + "}";
            }

            return actionName;
        }

        #endregion
    }
}
