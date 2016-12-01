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
using Microsoft.Extensions.Primitives;
using SimpleIdentityServer.Core.Api.Token;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.DTOs.Response;
using SimpleIdentityServer.Host.Extensions;
using System;
using System.Linq;
using System.Net.Http.Headers;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    [Route(Constants.EndPoints.Token)]
    public class TokenController : Controller
    {
        private readonly ITokenActions _tokenActions;

        public TokenController(
            ITokenActions tokenActions)
        {
            _tokenActions = tokenActions;
        }

        #region Public methods

        // [SimpleTypeFilterAttribute(typeof(RateLimitationFilterAttribute), Arguments = new object[] { "PostToken" })]
        [HttpPost]
        public TokenResponse Post([FromForm] TokenRequest tokenRequest)
        {
            GrantedToken result = null;
            StringValues authorizationHeader;
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out authorizationHeader)) 
            {
                var authorizationHeaderValue = authorizationHeader.First();
                var splittedAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splittedAuthorizationHeaderValue.Count() == 2)
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splittedAuthorizationHeaderValue[0],
                        splittedAuthorizationHeaderValue[1]);
                }
            }
            
            switch (tokenRequest.grant_type)
            {
                case GrantTypeRequest.password:
                    var resourceOwnerParameter = tokenRequest.ToResourceOwnerGrantTypeParameter();
                    result = _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(resourceOwnerParameter, authenticationHeaderValue);
                    break;
                case GrantTypeRequest.authorization_code:
                    var authCodeParameter = tokenRequest.ToAuthorizationCodeGrantTypeParameter();
                    result = _tokenActions.GetTokenByAuthorizationCodeGrantType(
                        authCodeParameter,
                        authenticationHeaderValue);
                    break;
                case GrantTypeRequest.refresh_token:
                    var refreshTokenParameter = tokenRequest.ToRefreshTokenGrantTypeParameter();
                    result = _tokenActions.GetTokenByRefreshTokenGrantType(refreshTokenParameter);
                    break;
                case GrantTypeRequest.client_credentials:
                    var clientCredentialsParameter = tokenRequest.ToClientCredentialsGrantTypeParameter();
                    result = _tokenActions.GetTokenByClientCredentialsGrantType(clientCredentialsParameter,
                        authenticationHeaderValue);
                    break;
            }

            return result.ToDto();
        }

        [HttpPost("revoke")]
        public ActionResult Post([FromForm] RevocationRequest revocationRequest)
        {
            if (revocationRequest == null)
            {
                throw new ArgumentNullException(nameof(revocationRequest));
            }

            // Fetch the authorization header
            StringValues authorizationHeader;
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out authorizationHeader))
            {
                var authorizationHeaderValue = authorizationHeader.First();
                var splittedAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splittedAuthorizationHeaderValue.Count() == 2)
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splittedAuthorizationHeaderValue[0],
                        splittedAuthorizationHeaderValue[1]);
                }
            }

            // Revoke the token
            _tokenActions.RevokeToken(revocationRequest.ToParameter(), authenticationHeaderValue);
            return new OkResult();
        }

        #endregion
    }
}
