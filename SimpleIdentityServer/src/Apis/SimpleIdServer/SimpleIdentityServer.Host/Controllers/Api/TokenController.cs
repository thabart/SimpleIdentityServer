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
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common.Serializers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Host.DTOs.Response;
using SimpleIdentityServer.Host.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

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

        // [SimpleTypeFilterAttribute(typeof(RateLimitationFilterAttribute), Arguments = new object[] { "PostToken" })]
        [HttpPost]
        public async Task<TokenResponse> PostToken()
        {
            var certificate = GetCertificate();
            if (Request.Form == null)
            {
                throw new ArgumentNullException(nameof(Request.Form));
            }

            var nameValueCollection = new NameValueCollection();
            foreach (var kvp in Request.Form)
            {
                nameValueCollection.Add(kvp.Key, kvp.Value);
            }

            var serializer = new ParamSerializer();
            var tokenRequest = serializer.Deserialize<TokenRequest>(nameValueCollection);
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
            
            switch (tokenRequest.GrantType)
            {
                case GrantTypes.password:
                    var resourceOwnerParameter = tokenRequest.ToResourceOwnerGrantTypeParameter();
                    result = await _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(resourceOwnerParameter, authenticationHeaderValue, certificate);
                    break;
                case GrantTypes.authorization_code:
                    var authCodeParameter = tokenRequest.ToAuthorizationCodeGrantTypeParameter();
                    result = await _tokenActions.GetTokenByAuthorizationCodeGrantType(
                        authCodeParameter,
                        authenticationHeaderValue);
                    break;
                case GrantTypes.refresh_token:
                    var refreshTokenParameter = tokenRequest.ToRefreshTokenGrantTypeParameter();
                    result = await _tokenActions.GetTokenByRefreshTokenGrantType(refreshTokenParameter);
                    break;
                case GrantTypes.client_credentials:
                    var clientCredentialsParameter = tokenRequest.ToClientCredentialsGrantTypeParameter();
                    result = await _tokenActions.GetTokenByClientCredentialsGrantType(clientCredentialsParameter, authenticationHeaderValue);
                    break;
            }

            return result.ToDto();
        }

        [HttpPost("revoke")]
        public async Task<ActionResult> PostRevoke()
        {
            if (Request.Form == null)
            {
                throw new ArgumentNullException(nameof(Request.Form));
            }

            var nameValueCollection = new NameValueCollection();
            foreach (var kvp in Request.Form)
            {
                nameValueCollection.Add(kvp.Key, kvp.Value);
            }

            var serializer = new ParamSerializer();
            var revocationRequest = serializer.Deserialize<RevocationRequest>(nameValueCollection);
            // 1. Fetch the authorization header
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

            // 2. Revoke the token
            await _tokenActions.RevokeToken(revocationRequest.ToParameter(), authenticationHeaderValue);
            return new OkResult();
        }

        private X509Certificate2 GetCertificate()
        {
            const string headerName = "X-ARR-ClientCert";
            var header = Request.Headers.FirstOrDefault(h => h.Key == headerName);
            if (header.Equals(default(KeyValuePair<string, StringValues>)))
            {
                return null;
            }

            try
            {
                var encoded = Convert.FromBase64String(header.Value);
                return new X509Certificate2(encoded);
            }
            catch
            {
                return null;
            }
        }
    }
}
