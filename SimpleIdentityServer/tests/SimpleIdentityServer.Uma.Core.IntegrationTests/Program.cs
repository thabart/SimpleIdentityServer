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

        private const string ClientId = "ManagerWebSite";

        private const string ClientSecret = "ManagerWebSite";

        private const string IdentityServerManagerClientId = "IdentityServerManager";

        private const string IdentityServerManagerClientSecret = "IdentityServerManager";

        private const string Login = "administrator";

        private const string Password = "password";

        private const string UmaProtectionScope = "uma_protection";

        private const string UmaAuthorizationScope = "uma_authorization";

        private const string UmaUrl = "http://localhost:5001";

        private const string AuthorizationUrl = "http://localhost:5000";

        #region Public static methods

        public static void Main(string[] args)
        {
            AuthorizedScenarioWithClaims();
            // GetRpt("36d318b5-6d6e-414d-a693-3970d3490b9d");
            // AuthorizedScenario();
            // AuthorizedScenarioByDiscovery();
        }

        #endregion

        #region Private static methods

        private static void GetRpt(string resourceSetId)
        {
            // 1. Get token valid for uma_protection scope
            var umaProtectionToken = GetGrantedToken(UmaProtectionScope);
            // 2. Get token valid for uma_authorization scope
            var umaAuthorizationToken = GetGrantedToken(UmaAuthorizationScope);
            // 3. Get a permission
            var permission = AddPermission(resourceSetId, new List<string> { "execute" }, umaProtectionToken.AccessToken);
            // 4. Get authorization
            var authorization = GetAuthorization(permission.TicketId, umaAuthorizationToken.AccessToken);
            Console.WriteLine(authorization.Rpt);
            Console.ReadLine();
        }

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
                var policy = GetPolicy(authorizationPolicy.PolicyId, umaProtectionToken.AccessToken);
                Console.WriteLine($"authorization policy {policy.Id} has been created");
                var policyIds = GetPolicies(umaProtectionToken.AccessToken);
                foreach (var policyId in policyIds)
                {
                    Console.WriteLine($"authorization policy : {policyId}");
                }

                // 6. Get authorization
                var authorization = GetAuthorization(permission.TicketId, umaAuthorizationToken.AccessToken);
                Console.WriteLine(authorization.Rpt);
                // 7. Introspect
                var introspection = GetIntrospection(authorization.Rpt);
                Console.WriteLine(introspection.IsActive);
                // 8. Drop everything
                var isAuthorizationPolicyDropped = DeletePolicy(authorizationPolicy.PolicyId, umaProtectionToken.AccessToken);
                Console.WriteLine($"authorization policy is dropped : {isAuthorizationPolicyDropped}");
            }
            catch (AggregateException ex)
            {
                var firstException = ex.InnerExceptions.First();
                Console.WriteLine(firstException.Message);
            }

            Console.ReadLine();
        }

        private static void AuthorizedScenarioByDiscovery()
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
                var resourceSet = AddResourceSetByDiscovery("resource_set", new List<string> { readScope, createScope }, umaProtectionToken.AccessToken);
                // 4. Add permission
                var permission = AddPermissionByDiscovery(resourceSet.Id, new List<string> { readScope, createScope }, umaProtectionToken.AccessToken);
                // 5. Add authorization policy
                var authorizationPolicy = AddPolicyByDiscovery(new List<string> { ClientId }, new List<string> { readScope, createScope }, resourceSet.Id, umaProtectionToken.AccessToken);
                var policy = GetPolicyByDiscovery(authorizationPolicy.PolicyId, umaProtectionToken.AccessToken);
                Console.WriteLine($"authorization policy {policy.Id} has been created");
                var policyIds = GetPoliciesByDiscovery(umaProtectionToken.AccessToken);
                foreach (var policyId in policyIds)
                {
                    Console.WriteLine($"authorization policy : {policyId}");
                }

                // 6. Get authorization
                var authorization = GetAuthorizationByDiscovery(permission.TicketId, umaAuthorizationToken.AccessToken);
                Console.WriteLine(authorization.Rpt);
                // 7. Introspect
                var introspection = GetIntrospectionByDiscovery(authorization.Rpt);
                Console.WriteLine(introspection.IsActive);
                // 8. Drop everything
                var isAuthorizationPolicyDropped = DeletePolicyByDiscovery(authorizationPolicy.PolicyId, umaProtectionToken.AccessToken);
                Console.WriteLine($"authorization policy is dropped : {isAuthorizationPolicyDropped}");
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

        private static void AuthorizedScenarioWithClaims()
        {
            _identityServerClientFactory = new IdentityServerClientFactory();
            const string readScope = "read";
            const string createScope = "create";
            // 1. Retrieve identity token
            var userToken = AuthenticateUser();
            // 2. Get token valid for uma_protection scope
            var umaProtectionToken = GetGrantedToken(UmaProtectionScope);
            // 3. Get token valid for uma_authorization scope
            var umaAuthorizationToken = GetGrantedToken(UmaAuthorizationScope);
            // 4. Add resource set
            // var resourceSet = AddResourceSet("resource_set", new List<string> { readScope, createScope }, umaProtectionToken.AccessToken);
            var resourceSetId = "153e2c10-8626-490f-a3e9-689fb6ae937c";
            // 5. Add authorization policy
            var authorizationPolicy = AddPolicy(
                new List<string> { ClientId }, 
                new List<string> { "execute" },
                resourceSetId, 
                umaProtectionToken.AccessToken,
                new List<PostClaim>
                {
                    new PostClaim
                    {
                        Type = "role",
                        Value = "administrator"
                    }
                });
            // 6. Add permission
            var permission = AddPermission(resourceSetId, new List<string> { "execute" }, umaProtectionToken.AccessToken);
            // 7. Get an authorization
            var authorization = GetAuthorization(
                permission.TicketId, 
                umaAuthorizationToken.AccessToken, 
                userToken.IdToken);
            Console.Write(authorization.Rpt);
            Console.ReadLine();
        }

        private static GrantedToken AuthenticateUser()
        {
            var result = _identityServerClientFactory.CreateTokenClient()
                .UseClientSecretPostAuth(IdentityServerManagerClientId, IdentityServerManagerClientSecret)
                .UsePassword(Login, Password, "openid", "role")
                .ResolveAsync(AuthorizationUrl + "/.well-known/openid-configuration")
                .Result;
            return result;
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

        private static AuthorizationResponse GetAuthorization(
            string ticketId, 
            string accessToken,
            string idToken = null)
        {
            var postAuthorization = new PostAuthorization
            {
                TicketId = ticketId
            };
            if (!string.IsNullOrWhiteSpace(idToken))
            {
                postAuthorization.ClaimTokens = new List<PostClaimToken>
                {
                    new PostClaimToken
                    {
                        Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                        Token = idToken
                    }
                };
            }

            var authorization = _identityServerUmaClientFactory.GetAuthorizationClient()
                .GetAuthorizationAsync(postAuthorization, UmaUrl + "/rpt", accessToken)
                .Result;
            return authorization;
        }

        private static AddPolicyResponse AddPolicy(
            List<string> clientIds, 
            List<string> scopes, 
            string resourceSetId, 
            string accessToken,
            List<PostClaim> claims = null)
        {
            var postPolicy = new PostPolicy
            {
                Scopes = scopes,
                ClientIdsAllowed = clientIds,
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                IsResourceOwnerConsentNeeded = false,
                Claims = claims
            };

            var policyResponse = _identityServerUmaClientFactory.GetPolicyClient()
                .AddPolicyAsync(postPolicy, UmaUrl + "/policies", accessToken)
                .Result;
            return policyResponse;
        }

        private static PolicyResponse GetPolicy(string policyId, string accessToken)
        {
            var policyResponse = _identityServerUmaClientFactory.GetPolicyClient()
                .GetPolicyAsync(policyId, UmaUrl + "/policies", accessToken)
                .Result;
            return policyResponse;
        }

        private static bool DeletePolicy(string policyId, string accessToken)
        {
            return _identityServerUmaClientFactory.GetPolicyClient()
                .DeletePolicyAsync(policyId, UmaUrl + "/policies", accessToken)
                .Result;
        }

        private static List<string> GetPolicies(string accessToken)
        {
            return _identityServerUmaClientFactory.GetPolicyClient()
                .GetPoliciesAsync(UmaUrl + "/policies", accessToken)
                .Result;
        }

        private static AddResourceSetResponse AddResourceSetByDiscovery(string resourceSetName, List<string> scopes, string accessToken)
        {
            var postResourceSet = new PostResourceSet
            {
                Name = resourceSetName,
                Scopes = scopes
            };
            var newResourceSet = _identityServerUmaClientFactory.GetResourceSetClient()
                .AddResourceSetByResolvingUrlAsync(postResourceSet, UmaUrl + "/.well-known/uma-configuration", accessToken)
                .Result;
            return newResourceSet;
        }

        private static AddPermissionResponse AddPermissionByDiscovery(string resourceSetId, List<string> scopes, string accessToken)
        {
            var postPermission = new PostPermission
            {
                ResourceSetId = resourceSetId,
                Scopes = scopes
            };
            var permission = _identityServerUmaClientFactory.GetPermissionClient()
                .AddPermissionByResolvingUrlAsync(postPermission, UmaUrl + "/.well-known/uma-configuration", accessToken)
                .Result;
            return permission;
        }

        private static AuthorizationResponse GetAuthorizationByDiscovery(string ticketId, string accessToken)
        {
            var postAuthorization = new PostAuthorization
            {
                TicketId = ticketId
            };
            var authorization = _identityServerUmaClientFactory.GetAuthorizationClient()
                .GetAuthorizationByResolvingUrlAsync(postAuthorization, UmaUrl + "/.well-known/uma-configuration", accessToken)
                .Result;
            return authorization;
        }

        private static AddPolicyResponse AddPolicyByDiscovery(List<string> clientIds, List<string> scopes, string resourceSetId, string accessToken)
        {
            var postPolicy = new PostPolicy
            {
                Scopes = scopes,
                ClientIdsAllowed = clientIds,
                ResourceSetIds = new List<string>
                {
                    resourceSetId
                },
                IsResourceOwnerConsentNeeded = false
            };

            var policyResponse = _identityServerUmaClientFactory.GetPolicyClient()
                .AddPolicyByResolvingUrlAsync(postPolicy, UmaUrl + "/.well-known/uma-configuration", accessToken)
                .Result;
            return policyResponse;
        }

        private static PolicyResponse GetPolicyByDiscovery(string policyId, string accessToken)
        {
            var policyResponse = _identityServerUmaClientFactory.GetPolicyClient()
                .GetPolicyByResolvingUrlAsync(policyId, UmaUrl + "/.well-known/uma-configuration", accessToken)
                .Result;
            return policyResponse;
        }

        private static bool DeletePolicyByDiscovery(string policyId, string accessToken)
        {
            return _identityServerUmaClientFactory.GetPolicyClient()
                .DeletePolicyByResolvingUrlAsync(policyId, UmaUrl + "/.well-known/uma-configuration", accessToken)
                .Result;
        }

        private static List<string> GetPoliciesByDiscovery(string accessToken)
        {
            return _identityServerUmaClientFactory.GetPolicyClient()
                .GetPoliciesByResolvingUrlAsync(UmaUrl + "/.well-known/uma-configuration", accessToken)
                .Result;
        }

        private static IntrospectionResponse GetIntrospection(string rpt)
        {
            return _identityServerUmaClientFactory.GetIntrospectionClient()
                .GetIntrospectionAsync(rpt, UmaUrl + "/status")
                .Result;
        }

        private static IntrospectionResponse GetIntrospectionByDiscovery(string rpt)
        {
            return _identityServerUmaClientFactory.GetIntrospectionClient()
                .GetIntrospectionByResolvingUrlAsync(rpt, UmaUrl + "/.well-known/uma-configuration")
                .Result;
        }

        #endregion
    }
}
