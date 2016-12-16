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
using SimpleIdentityServer.Uma.Common.DTOs;
using SimpleIdentityServer.UmaManager.Client;
using SimpleIdentityServer.UmaManager.Client.DTOs.Requests;
using SimpleIdentityServer.UmaManager.Client.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Proxy
{
    public interface ISecurityProxy
    {
        Task<string> GetRpt(string resourceUrl,
            string umaProtectionToken,
            string umaAuthorizationToken,
            string resourceToken,
            List<string> permissions);

        Task<string> GetRpt(string resourceUrl,
            string identityToken,
            string umaProtectionToken,
            string umaAuthorizationToken,
            string resourceToken,
            List<string> permissions);
    }

    internal class SecurityProxy : ISecurityProxy
    {
        private const string UmaProtectionScope = "uma_protection";
        private const string UmaAuthorizationScope = "uma_authorization";
        private readonly SecurityOptions _securityOptions;
        private readonly IIdentityServerClientFactory _identityServerClientFactory;
        private readonly IIdentityServerUmaManagerClientFactory _identityServerUmaManagerClientFactory;
        private readonly IIdentityServerUmaClientFactory _identityServerUmaClientFactory;

        public SecurityProxy(
            SecurityOptions securityOptions,
            IIdentityServerClientFactory identityServerClientFactory,
            IIdentityServerUmaManagerClientFactory identityServerUmaManagerClientFactory,
            IIdentityServerUmaClientFactory identityServerUmaClientFactory)
        {
            _securityOptions = securityOptions;
            _identityServerClientFactory = identityServerClientFactory;
            _identityServerUmaManagerClientFactory = identityServerUmaManagerClientFactory;
            _identityServerUmaClientFactory = identityServerUmaClientFactory;
        }


        public async Task<string> GetRpt(string resourceUrl,
            string umaProtectionToken,
            string umaAuthorizationToken,
            string resourceToken,
            List<string> permissions)
        {
            if (string.IsNullOrWhiteSpace(umaProtectionToken))
            {
                throw new ArgumentNullException(nameof(umaProtectionToken));
            }

            if (string.IsNullOrWhiteSpace(umaAuthorizationToken))
            {
                throw new ArgumentNullException(nameof(umaAuthorizationToken));
            }

            Func<string, string, Task<string>> callback = new Func<string, string, Task<string>>(async (t, a) => {
                AuthorizationResponse resp = null;
                try
                {
                    resp = await GetAuthorization(t, a);
                }
                catch
                {
                    throw new InvalidOperationException("you're not allowed to access to the resource");
                }

                return resp.Rpt;
            });

            return await CommonGetRpt(
                resourceUrl, 
                permissions, 
                umaAuthorizationToken, 
                umaProtectionToken,
                resourceToken,
                callback);
        }

        public async Task<string> GetRpt(string resourceUrl,
            string identityToken,
            string umaProtectionToken,
            string umaAuthorizationToken,
            string resourceToken,
            List<string> permissions)
        {
            if (string.IsNullOrWhiteSpace(identityToken))
            {
                throw new ArgumentNullException(nameof(identityToken));
            }

            if (string.IsNullOrWhiteSpace(umaProtectionToken))
            {
                throw new ArgumentNullException(nameof(umaProtectionToken));
            }

            if (string.IsNullOrWhiteSpace(umaAuthorizationToken))
            {
                throw new ArgumentNullException(nameof(umaAuthorizationToken));
            }

            Func<string, string, Task<string>> callback = new Func<string, string, Task<string>>(async (t, a) => {
                AuthorizationResponse resp = null;
                try
                {
                    resp = await GetAuthorization(t, a, identityToken);
                }
                catch
                {
                    throw new InvalidOperationException("you're not allowed to access to the resource");
                }

                return resp.Rpt;
            });

            return await CommonGetRpt(
                resourceUrl, 
                permissions, 
                umaAuthorizationToken, 
                umaProtectionToken,
                resourceToken,
                callback);
        }
        
        private async Task<string> CommonGetRpt(string resourceUrl,
            List<string> permissions,
            string umaAuthorizationToken,
            string umaProtectionToken,
            string resourceToken,
            Func<string, string, Task<string>> callback)
        {
            if (string.IsNullOrWhiteSpace(resourceUrl))
            {
                throw new ArgumentNullException(nameof(resourceUrl));
            }

            if (permissions == null || !permissions.Any())
            {
                throw new ArgumentNullException(nameof(permissions));
            }

            // 1. Get resource
            var resource = await GetResource(resourceUrl, resourceToken);
            // 2. Get permission
            AddPermissionResponse permission;
            try
            {
                permission = await AddPermission(resource.ResourceSetId, permissions, umaProtectionToken);
            }
            catch
            {
                throw new InvalidOperationException("Either the access token is wrong, resource id doesn't exist or permissions are not correct");
            }

            return await callback(permission.TicketId, umaAuthorizationToken);
        }

        private async Task<ResourceResponse> GetResource(string req, string resourceToken)
        {
            var url = $"{_securityOptions.RootManageApiUrl.TrimEnd('/')}/vs/resources";
            var resources = await _identityServerUmaManagerClientFactory.GetResourceClient()
                .SearchResources(new SearchResourceRequest
                {
                    Url = req
                }, new Uri(url), resourceToken);
            if (resources == null)
            {
                throw new InvalidOperationException("the resource doesn't exist");
            }

            var resource = resources.FirstOrDefault(r => r.Url == req);
            if (resource == null)
            {
                throw new InvalidOperationException("the resource doesn't exist");
            }

            return resource;
        }

        private async Task<AddPermissionResponse> AddPermission(
            string resourceSetId,
            List<string> scopes,
            string accessToken)
        {
            var postPermission = new PostPermission
            {
                ResourceSetId = resourceSetId,
                Scopes = scopes
            };

            return await _identityServerUmaClientFactory.GetPermissionClient().AddByResolution(postPermission, _securityOptions.UmaConfigurationUrl, accessToken);
        }

        private async Task<AuthorizationResponse> GetAuthorization(
            string ticketId,
            string accessToken)
        {
            var postAuthorization = new PostAuthorization
            {
                TicketId = ticketId
            };

            return await _identityServerUmaClientFactory.GetAuthorizationClient()
                .GetByResolution(postAuthorization, _securityOptions.UmaConfigurationUrl, accessToken);
        }

        private async Task<AuthorizationResponse> GetAuthorization(
            string ticketId,
            string accessToken,
            string identityToken)
        {
            var postAuthorization = new PostAuthorization
            {
                TicketId = ticketId,
                ClaimTokens = new List<PostClaimToken>
                {
                    new PostClaimToken
                    {
                        Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                        Token = identityToken
                    }
                }
            };

            return await _identityServerUmaClientFactory.GetAuthorizationClient()
                .GetByResolution(postAuthorization, _securityOptions.UmaConfigurationUrl, accessToken);
        }
    }
}
