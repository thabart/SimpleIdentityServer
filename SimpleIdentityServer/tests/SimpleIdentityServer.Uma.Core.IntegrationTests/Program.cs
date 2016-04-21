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
using SimpleIdentityServer.Client.DTOs.Response;
using SimpleIdentityServer.Client.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Uma.Core.IntegrationTests
{
    class Program
    {
        private static IdentityServerClientFactory _identityServerClientFactory = new IdentityServerClientFactory();

        private static IdentityServerUmaClientFactory _identityServerUmaClientFactory = new IdentityServerUmaClientFactory();

        private const string ClientId = "UmaResourceServer";

        private const string ClientSecret = "UmaResourceServer";

        private const string UmaProtectionScope = "uma_protection";

        private const string UmaAuthorizationScope = "uma_authorization";

        private const string UmaUrl = "http://localhost:5002";

        private const string AuthorizationUrl = "http://localhost:5000";

        #region Public static methods

        public static void Main(string[] args)
        {
            AuthorizedScenario();
        }

        #endregion

        #region Private static methods

        private static void AuthorizedScenario()
        {
            try
            {
                _identityServerClientFactory = new IdentityServerClientFactory();
                const string readScope = "read";
                const string createScope = "create";
                // 1. Get token valid for uma_protection scope
                var umaProtectionToken = GetGrantedToken(UmaProtectionScope);
                // 2. Get token vaild for uma_authorization scope
                var umaAuthorizationToken = GetGrantedToken(UmaAuthorizationScope);
                // 3. Add resource set
                var resourceSet = AddResourceSet("resource_set", new List<string> { readScope, createScope }, umaProtectionToken.AccessToken);
                // 4. Add permission
                var permission = AddPermission(resourceSet.Id, new List<string> { readScope, createScope }, umaProtectionToken.AccessToken);
                // 5. Add authorization policy
                var authorizationPolicy = AddPolicy(new List<string> { ClientId }, new List<string> { readScope, createScope }, resourceSet.Id, umaProtectionToken.AccessToken);
                // 6. Get authorization
                var authorization = GetAuthorization(permission.TicketId, umaAuthorizationToken.AccessToken);
                Console.Write(authorization.Rpt);
            }
            catch (AggregateException ex)
            {
                var firstException = ex.InnerExceptions.First();
                Console.WriteLine(firstException.Message);
            }

            Console.ReadLine();
        }

        private static void UnauthorizedScenario()
        {
            try
            {
                _identityServerClientFactory = new IdentityServerClientFactory();
                const string readScope = "read";
                const string createScope = "create";
                // 1. Get token valid for uma_protection scope
                var umaProtectionToken = GetGrantedToken(UmaProtectionScope);
                // 2. Get token vaild for uma_authorization scope
                var umaAuthorizationToken = GetGrantedToken(UmaAuthorizationScope);
                // 3. Add resource set
                var resourceSet = AddResourceSet("resource_set", new List<string> { readScope, createScope }, umaProtectionToken.AccessToken);
                // 4. Add permission
                var permission = AddPermission(resourceSet.Id, new List<string> { createScope }, umaProtectionToken.AccessToken);
                // 5. Add authorization policy
                var authorizationPolicy = AddPolicy(new List<string> { ClientId }, new List<string> { readScope }, resourceSet.Id, umaProtectionToken.AccessToken);
                // 6. Get authorization
                var authorization = GetAuthorization(permission.TicketId, umaAuthorizationToken.AccessToken);
                Console.Write(authorization.Rpt);
            }
            catch (AggregateException ex)
            {
                var firstException = ex.InnerExceptions.First();
                Console.WriteLine(firstException.Message);
            }

            Console.ReadLine();
        }

        private static GrantedToken GetGrantedToken(string scope)
        {
            var result = _identityServerClientFactory.CreateTokenClient()
                .UseClientSecretPostAuth(ClientId, ClientSecret)
                .UseClientCredentials(scope)
                .ResolveAsync(AuthorizationUrl + "/.well-known/openid-configuration")
                .Result;
            return result;
        }

        private static AddResourceSetResponse AddResourceSet(string resourceSetName, List<string> scopes, string accessToken)
        {
            var postResourceSet = new PostResourceSet
            {
                Name = resourceSetName,
                Scopes = scopes
            };
            var newResourceSet = _identityServerUmaClientFactory.GetResourceSetClient()
                .AddResourceSetAsync(postResourceSet, UmaUrl + "/rs/resource_set", accessToken)
                .Result;
            return newResourceSet;
        }

        private static AddPermissionResponse AddPermission(string resourceSetId, List<string> scopes, string accessToken)
        {
            var postPermission = new PostPermission
            {
                ResourceSetId = resourceSetId,
                Scopes = scopes
            };
            var permission = _identityServerUmaClientFactory.GetPermissionClient()
                .AddPermissionAsync(postPermission, UmaUrl + "/perm", accessToken)
                .Result;
            return permission;
        }

        private static AuthorizationResponse GetAuthorization(string ticketId, string accessToken)
        {
            var postAuthorization = new PostAuthorization
            {
                TicketId = ticketId
            };
            var authorization = _identityServerUmaClientFactory.GetAuthorizationClient()
                .GetAuthorizationAsync(postAuthorization, UmaUrl + "/rpt", accessToken)
                .Result;
            return authorization;
        }

        private static AddPolicyResponse AddPolicy(List<string> clientIds, List<string> scopes, string resourceSetId, string accessToken)
        {
            var postPolicy = new PostPolicy
            {
                IsCustom = false,
                Scopes = scopes,
                ClientIdsAllowed = clientIds,
                ResourceSetId = resourceSetId,
                IsResourceOwnerConsentNeeded = false
            };

            var policyResponse = _identityServerUmaClientFactory.GetPolicyClient()
                .AddPolicyAsync(postPolicy, UmaUrl + "/policies", accessToken)
                .Result;
            return policyResponse;
        }

        #endregion
    }
}
