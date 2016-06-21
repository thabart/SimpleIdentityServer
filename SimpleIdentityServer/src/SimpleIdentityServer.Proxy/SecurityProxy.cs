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
using SimpleIdentityServer.UmaManager.Client;
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
            params string[] permissions);

        Task<string> GetRpt(string resourceUrl,
            List<string> permissions,
            string identityToken);

        Task<string> GetRpt(string resourceUrl,
            string identityToken,
            params string[] permissions);

        Task<string> GetRpt(string resourceUrl,
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

        #region Constructor

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

        #endregion

        #region Public methods

        public async Task<string> GetRpt(string resourceUrl,
            params string[] permissions)
        {
            if (permissions == null)
            {
                throw new ArgumentNullException(nameof(permissions));
            }

            return await GetRpt(resourceUrl, permissions.ToList());
        }

        public async Task<string> GetRpt(string resourceUrl,
            List<string> permissions)
        {
            Func<string, string, Task<string>> callback = new Func<string, string, Task<string>>(async (t, a) => {
                AuthorizationResponse resp = await GetAuthorization(t, a);
                return resp.Rpt;
            });

            return await CommonGetRpt(resourceUrl, permissions, callback);
        }

        public async Task<string> GetRpt(string resourceUrl,
            string identityToken,
            params string[] permissions)
        {
            if (permissions == null)
            {
                throw new ArgumentNullException(nameof(permissions));
            }

            return await GetRpt(resourceUrl, permissions.ToList(), identityToken);
        }

        public async Task<string> GetRpt(string resourceUrl,
            List<string> permissions,
            string identityToken)
        {
            Func<string, string, Task<string>> callback = new Func<string, string, Task<string>>(async (t, a) => {
                AuthorizationResponse resp = await GetAuthorization(t, a, identityToken);
                return resp.Rpt;
            });

            return await CommonGetRpt(resourceUrl, permissions, callback);
        }

        #endregion

        #region Private methods

        private async Task<string> CommonGetRpt(string resourceUrl,
            List<string> permissions,
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

            IEnumerable<GrantedToken> result = await Task.WhenAll(GetGrantedToken(UmaProtectionScope),
                GetGrantedToken(UmaAuthorizationScope));
            var tokens = result.ToList();
            GrantedToken umaProtectionToken = tokens[0],
                umaAuthorizationToken = tokens[1];
            // 1. Get resource
            var resource = await GetResource(resourceUrl);
            // 2. Get permission
            var permission = await AddPermission(resource.ResourceSetId, permissions, umaProtectionToken.AccessToken);
            return await callback(permission.TicketId, umaAuthorizationToken.AccessToken);
        }

        private Task<GrantedToken> GetGrantedToken(string scope)
        {
            return _identityServerClientFactory.CreateTokenClient()
                .UseClientSecretPostAuth(_securityOptions.ClientId, _securityOptions.ClientSecret)
                .UseClientCredentials(scope)
                .ResolveAsync(_securityOptions.OpenidConfigurationUrl);
        }

        private async Task<ResourceResponse> GetResource(string query)
        {
            var url = $"{_securityOptions.RootManageApiUrl.TrimEnd('/')}/vs/resources";
            var resources = await _identityServerUmaManagerClientFactory.GetResourceClient()
                .SearchResources(query, new Uri(url), string.Empty);
            if (resources == null)
            {
                throw new InvalidOperationException("the resource doesn't exist");
            }

            var resource = resources.FirstOrDefault(r => r.Url == query);
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

            return await _identityServerUmaClientFactory.GetPermissionClient()
                .AddPermissionByResolvingUrlAsync(postPermission, _securityOptions.UmaConfigurationUrl, accessToken);
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
                .GetAuthorizationByResolvingUrlAsync(postAuthorization, _securityOptions.UmaConfigurationUrl, accessToken);
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
                .GetAuthorizationByResolvingUrlAsync(postAuthorization, _securityOptions.UmaConfigurationUrl, accessToken);
        }

        #endregion
    }
}
