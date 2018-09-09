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

using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Api.Discovery;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Host.Extensions;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    [Route(Constants.EndPoints.DiscoveryAction)]
    public class DiscoveryController : Controller
    {
        private readonly IDiscoveryActions _discoveryActions;
        private readonly ScimOptions _scim;

        public DiscoveryController(IDiscoveryActions discoveryActions, ScimOptions scim)
        {
            _discoveryActions = discoveryActions;
            _scim = scim;
        }

        [HttpGet]
        public async Task<DiscoveryInformation> Get()
        {
            return await GetMetadata().ConfigureAwait(false);
        }

        private async Task<DiscoveryInformation> GetMetadata()
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var authorizationEndPoint = issuer + "/" + Constants.EndPoints.Authorization;
            var tokenEndPoint = issuer + "/" + Constants.EndPoints.Token;
            var userInfoEndPoint = issuer + "/" + Constants.EndPoints.UserInfo;
            var jwksUri = issuer + "/" + Constants.EndPoints.Jwks;
            var registrationEndPoint = issuer + "/" + Constants.EndPoints.Registration;
            var revocationEndPoint = issuer + "/" + Constants.EndPoints.Revocation;
            // TODO : implement the session management : http://openid.net/specs/openid-connect-session-1_0.html
            var checkSessionIframe = issuer + "/" + Constants.EndPoints.CheckSession;
            var endSessionEndPoint = issuer + "/" + Constants.EndPoints.EndSession;
            var introspectionEndPoint = issuer + "/" + Constants.EndPoints.Introspection;

            var result = await _discoveryActions.CreateDiscoveryInformation().ConfigureAwait(false);
            result.Issuer = issuer;
            result.AuthorizationEndPoint = authorizationEndPoint;
            result.TokenEndPoint = tokenEndPoint;
            result.UserInfoEndPoint = userInfoEndPoint;
            result.JwksUri = jwksUri;
            result.RegistrationEndPoint = registrationEndPoint;
            result.RevocationEndPoint = revocationEndPoint;
            result.IntrospectionEndPoint = introspectionEndPoint;
            result.Version = "1.0";
            result.CheckSessionEndPoint = checkSessionIframe;
            result.EndSessionEndPoint = endSessionEndPoint;
            if (_scim.IsEnabled)
            {
                result.ScimEndpoint = _scim.EndPoint;
            }

            return result;
        }
    }
}
