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
            List<string> permissions);

        Task<string> GetRpt(string resourceUrl,
            string identityToken,
            params string[] permissions);

        Task<string> GetRpt(string resourceUrl,
            string identityToken,
            List<string> permissions);

        Task<string> GetRpt(string resourceUrl,
            string umaProtectionToken,
            string umaAuthorizationToken,
            List<string> permissions);

        Task<string> GetRpt(string resourceUrl,
            string identityToken,
            string umaProtectionToken,
            string umaAuthorizationToken,
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
            List<string> permissions)
        {
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

            var tokens = await GetAccessTokens();
            return await CommonGetRpt(resourceUrl, permissions, tokens.Value, tokens.Key, callback);
        }

        /// <returns></returns>
        public async Task<string> GetRpt(string resourceUrl, 
            string identityToken,
            params string[] permissions)
        {
            if (permissions == null)
            {
                throw new ArgumentNullException(nameof(permissions));
            }
            
            return await GetRpt(resourceUrl, permissions.ToList());
        }

        public async Task<string> GetRpt(string resourceUrl,
            string identityToken,
            List<string> permissions)
        {
            if (string.IsNullOrWhiteSpace(identityToken))
            {
                throw new ArgumentNullException(nameof(identityToken));
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

            var tokens = await GetAccessTokens();
            return await CommonGetRpt(resourceUrl, permissions, tokens.Value, tokens.Key, callback);
        }

        public async Task<string> GetRpt(string resourceUrl,
            string umaProtectionToken,
            string umaAuthorizationToken,
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

            return await CommonGetRpt(resourceUrl, permissions, umaAuthorizationToken, umaProtectionToken, callback);
        }

        public async Task<string> GetRpt(string resourceUrl,
            string identityToken,
            string umaProtectionToken,
            string umaAuthorizationToken,
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

            return await CommonGetRpt(resourceUrl, permissions, umaAuthorizationToken, umaProtectionToken, callback);
        }        

        #endregion

        #region Private methods

        private async Task<KeyValuePair<string, string>> GetAccessTokens()
        {

            GrantedToken umaProtectionToken = null,
                umaAuthorizationToken = null;
            try
            {
                umaProtectionToken = await GetGrantedToken(UmaProtectionScope);
            }
            catch
            {
                throw new InvalidOperationException($"Access token valid for {UmaProtectionScope} cannot be retrieved. Client is either invalid or is not allowed to access");
            }

            try
            {
                umaAuthorizationToken = await GetGrantedToken(UmaAuthorizationScope);
            }
            catch
            {
                throw new InvalidOperationException($"Access token valid for {UmaAuthorizationScope} cannot be retrieved. Client is either invalid or is not allowed to access");
            }

            return new KeyValuePair<string, string>(umaProtectionToken.AccessToken, umaAuthorizationToken.AccessToken);
        }

        private async Task<string> CommonGetRpt(string resourceUrl,
            List<string> permissions,
            string umaAuthorizationToken,
            string umaProtectionToken,
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
            var resource = await GetResource(resourceUrl);
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

        private Task<GrantedToken> GetGrantedToken(string scope)
        {
            return _identityServerClientFactory.CreateTokenClient()
                .UseClientSecretPostAuth(_securityOptions.ClientId, _securityOptions.ClientSecret)
                .UseClientCredentials(scope)
                .ResolveAsync(_securityOptions.OpenidConfigurationUrl);
        }

        private async Task<ResourceResponse> GetResource(string req)
        {
            var url = $"{_securityOptions.RootManageApiUrl.TrimEnd('/')}/vs/resources";
            var resources = await _identityServerUmaManagerClientFactory.GetResourceClient()
                .SearchResources(new SearchResourceRequest
                {
                    Url = req
                }, new Uri(url), string.Empty);
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
