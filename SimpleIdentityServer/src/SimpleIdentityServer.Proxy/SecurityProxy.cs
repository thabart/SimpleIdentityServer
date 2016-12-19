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
        Task<IEnumerable<string>> GetRpts(IEnumerable<string> resourceIds, string umaProtectionToken, string umaAuthorizationToken, IEnumerable<string> permissions);
        Task<IEnumerable<string>> GetRpts(IEnumerable<string> resourceIds, string identityToken, string umaProtectionToken, string umaAuthorizationToken, IEnumerable<string> permissions);
        Task<string> GetRpt(string resourceId, string umaProtectionToken, string umaAuthorizationToken, IEnumerable<string> permissions);
        Task<string> GetRpt(string resourceId, string identityToken, string umaProtectionToken, string umaAuthorizationToken, IEnumerable<string> permissions);
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

        public async Task<IEnumerable<string>> GetRpts(IEnumerable<string> resourceIds, string umaProtectionToken, string umaAuthorizationToken, IEnumerable<string> permissions)
        {
            if (string.IsNullOrWhiteSpace(umaProtectionToken))
            {
                throw new ArgumentNullException(nameof(umaProtectionToken));
            }

            if (string.IsNullOrWhiteSpace(umaAuthorizationToken))
            {
                throw new ArgumentNullException(nameof(umaAuthorizationToken));
            }

            Func<IEnumerable<string>, string, Task<IEnumerable<string>>> callback = new Func<IEnumerable<string>, string, Task<IEnumerable<string>>>(async (t, a) => {
                BulkAuthorizationResponse resp = null;
                try
                {
                    resp = await GetAuthorizations(t, a);
                }
                catch
                {
                    throw new InvalidOperationException("you're not allowed to access to the resource");
                }

                return resp.Rpts;
            });

            return await CommonGetRpts(resourceIds, permissions, umaAuthorizationToken, umaProtectionToken, callback);
        }

        public async Task<IEnumerable<string>> GetRpts(IEnumerable<string> resourceIds, string identityToken, string umaProtectionToken, string umaAuthorizationToken, IEnumerable<string> permissions)
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

            Func<IEnumerable<string>, string, Task<IEnumerable<string>>> callback = new Func<IEnumerable<string>, string, Task<IEnumerable<string>>>(async (t, a) => {
                BulkAuthorizationResponse resp = null;
                try
                {
                    resp = await GetAuthorizations(t, a, identityToken);
                }
                catch
                {
                    throw new InvalidOperationException("you're not allowed to access to the resource");
                }

                return resp.Rpts;
            });

            return await CommonGetRpts(resourceIds, permissions, umaAuthorizationToken, umaProtectionToken, callback);
        }

        public async Task<string> GetRpt(string resourceId, string umaProtectionToken, string umaAuthorizationToken, IEnumerable<string> permissions)
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

            return await CommonGetRpt(resourceId, permissions, umaAuthorizationToken, umaProtectionToken, callback);
        }

        public async Task<string> GetRpt(string resourceId, string identityToken, string umaProtectionToken, string umaAuthorizationToken, IEnumerable<string> permissions)
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

            return await CommonGetRpt(resourceId, permissions, umaAuthorizationToken, umaProtectionToken, callback);
        }
        
        private async Task<string> CommonGetRpt(string resourceId, IEnumerable<string> permissions, string umaAuthorizationToken, string umaProtectionToken, Func<string, string, Task<string>> callback)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            if (permissions == null || !permissions.Any())
            {
                throw new ArgumentNullException(nameof(permissions));
            }
            
            // 1. Add permission
            AddPermissionResponse permission;
            try
            {
                permission = await AddPermission(resourceId, permissions, umaProtectionToken);
            }
            catch
            {
                throw new InvalidOperationException("Either the access token is wrong, resource id doesn't exist or permissions are not correct");
            }

            return await callback(permission.TicketId, umaAuthorizationToken);
        }

        private async Task<IEnumerable<string>> CommonGetRpts(IEnumerable<string> resourceIds, IEnumerable<string> permissions, string umaAuthorizationToken, string umaProtectionToken, Func<IEnumerable<string>, string, Task<IEnumerable<string>>> callback)
        {
            if (resourceIds == null)
            {
                throw new ArgumentNullException(nameof(resourceIds));
            }

            if (permissions == null || !permissions.Any())
            {
                throw new ArgumentNullException(nameof(permissions));
            }

            // 1. Add permission
            AddPermissionsResponse permission;
            try
            {
                permission = await AddPermissions(resourceIds, permissions, umaProtectionToken);
            }
            catch
            {
                throw new InvalidOperationException("Either the access token is wrong, resource id doesn't exist or permissions are not correct");
            }

            return await callback(permission.TicketIds, umaAuthorizationToken);
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

        private async Task<AddPermissionResponse> AddPermission(string resourceId, IEnumerable<string> scopes, string accessToken)
        {
            var postPermission = new PostPermission
            {
                ResourceSetId = resourceId,
                Scopes = scopes
            };

            return await _identityServerUmaClientFactory.GetPermissionClient().AddByResolution(postPermission, _securityOptions.UmaConfigurationUrl, accessToken);
        }
        
        private async Task<AddPermissionsResponse> AddPermissions(IEnumerable<string> resourceIds, IEnumerable<string> scopes, string accessToken)
        {
            var permissions = resourceIds.Select(i => new PostPermission
            {
                ResourceSetId = i,
                Scopes = scopes
            });

            return await _identityServerUmaClientFactory.GetPermissionClient().AddByResolution(permissions, _securityOptions.UmaConfigurationUrl, accessToken);
        }

        private async Task<AuthorizationResponse> GetAuthorization(string ticketId, string accessToken)
        {
            var postAuthorization = new PostAuthorization
            {
                TicketId = ticketId
            };

            return await _identityServerUmaClientFactory.GetAuthorizationClient()
                .GetByResolution(postAuthorization, _securityOptions.UmaConfigurationUrl, accessToken);
        }

        private async Task<BulkAuthorizationResponse> GetAuthorizations(IEnumerable<string> ticketIds, string accessToken)
        {
            var requests = ticketIds.Select(t => new PostAuthorization { TicketId = t });
            return await _identityServerUmaClientFactory.GetAuthorizationClient()
                .GetByResolution(requests, _securityOptions.UmaConfigurationUrl, accessToken);
        }

        private async Task<BulkAuthorizationResponse> GetAuthorizations(IEnumerable<string> ticketIds, string accessToken, string identityToken)
        {
            var requests = ticketIds.Select(t => new PostAuthorization
            {
                TicketId = t,
                ClaimTokens = new List<PostClaimToken>
                {
                    new PostClaimToken
                    {
                        Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                        Token = identityToken
                    }
                }
            });
            return await _identityServerUmaClientFactory.GetAuthorizationClient()
                .GetByResolution(requests, _securityOptions.UmaConfigurationUrl, accessToken);
        }

        private async Task<AuthorizationResponse> GetAuthorization(string ticketId, string accessToken, string identityToken)
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
