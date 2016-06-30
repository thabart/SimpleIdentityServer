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
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Uma.Authorization
{
    public static class AuthorizationPolicyBuilderExtensions
    {
        #region Public static methods

        public static AuthorizationPolicyBuilder AddConventionalUma(
            this AuthorizationPolicyBuilder authorizationPolicyBuilder,
            ConventionalUmaOptions conventionalUmaOptions = null)
        {
            if (authorizationPolicyBuilder == null)
            {
                throw new ArgumentNullException(nameof(authorizationPolicyBuilder));
            }

            authorizationPolicyBuilder.Requirements.Add(new ConventionalUmaAuthorizationRequirement(conventionalUmaOptions));
            return authorizationPolicyBuilder;
        }

        public static AuthorizationPolicyBuilder AddResourceUma(
            this AuthorizationPolicyBuilder authorizationPolicyBuilder,
            string url,
            List<string> scopes)
        {
            if (authorizationPolicyBuilder == null)
            {
                throw new ArgumentNullException(nameof(authorizationPolicyBuilder));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (scopes == null || !scopes.Any())
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            authorizationPolicyBuilder.Requirements.Add(new ResourceUmaAuthorizationRequirement(url, scopes));
            return authorizationPolicyBuilder;
        }

        #endregion
    }
}
