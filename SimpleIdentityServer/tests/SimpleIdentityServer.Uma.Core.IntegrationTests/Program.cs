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

using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.DTOs.Requests;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Core.IntegrationTests
{
    class Program
    {
        #region Public methods

        public static void Main(string[] args)
        {
            // 1. Get access token
            var identityServerClientFactory = new IdentityServerClientFactory();
            var result = identityServerClientFactory.CreateTokenClient()
                .UseClientSecretPostAuth("UmaResourceServer", "UmaResourceServer")
                .UseClientCredentials("uma_protection")
                .ResolveAsync("http://localhost:5000/.well-known/openid-configuration")
                .Result;

            // 2. Add resource set
            var identityServerUmaClientFactory = new IdentityServerUmaClientFactory();
            var postResourceSet = new PostResourceSet
            {
                Name = "resource_set_name",
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            var newResourceSet = identityServerUmaClientFactory.GetResourceSetClient()
                .AddResourceSetAsync(postResourceSet, "http://localhost:5002/rs/resource_set", result.AccessToken)
                .Result;

            // 3. Add permission
            var postPermission = new PostPermission
            {
                ResourceSetId = newResourceSet.Id,
                Scopes = new List<string>
                {
                    "scope"
                }
            };
            var permission = identityServerUmaClientFactory.GetPermissionClient()
                .AddPermissionAsync(postPermission, "http://localhost:5002/perm", result.AccessToken)
                .Result;
            Console.ReadLine();
        }
        
        #endregion
    }
}
