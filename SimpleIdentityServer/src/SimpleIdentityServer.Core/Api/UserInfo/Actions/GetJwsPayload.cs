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

using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;
using System;
using Microsoft.AspNet.Mvc;
using System.Net;
using Microsoft.AspNet.Mvc.Formatters;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;

namespace SimpleIdentityServer.Core.Api.UserInfo.Actions
{
    public interface IGetJwsPayload
    {
        UserInfoResult Execute(string accessToken);
    }

    public class GetJwsPayload : IGetJwsPayload
    {
        private readonly IGrantedTokenValidator _grantedTokenValidator;

        private readonly IGrantedTokenRepository _grantedTokenRepository;

        private readonly IJwtGenerator _jwtGenerator;

        private readonly IClientRepository _clientRepository;

        public GetJwsPayload(
            IGrantedTokenValidator grantedTokenValidator,
            IGrantedTokenRepository grantedTokenRepository,
            IJwtGenerator jwtGenerator,
            IClientRepository clientRepository)
        {
            _grantedTokenValidator = grantedTokenValidator;
            _grantedTokenRepository = grantedTokenRepository;
            _jwtGenerator = jwtGenerator;
            _clientRepository = clientRepository;
        }

        public UserInfoResult Execute(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException("accessToken");
            }

            string messageErrorCode;
            string messageErrorDescription;
            // Check if the access token is still valid otherwise raise an authorization exception.
            if (!_grantedTokenValidator.CheckAccessToken(
                accessToken, 
                out messageErrorCode, 
                out messageErrorDescription))
            {
                throw new AuthorizationException(messageErrorCode, messageErrorDescription);
            }

            var grantedToken = _grantedTokenRepository.GetToken(accessToken);
            var client = _clientRepository.GetClientById(grantedToken.ClientId);
            var signedResponseAlg = client.GetUserInfoSignedResponseAlg();
            var userInformationPayload = grantedToken.UserInfoPayLoad;
            if (signedResponseAlg == null ||
                signedResponseAlg.Value == JwsAlg.none)
            {
                return new UserInfoResult
                {
                    Content = new ObjectResult(grantedToken.UserInfoPayLoad)
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Formatters = new List<IOutputFormatter>
                        {
                            new JsonOutputFormatter()
                        },
                        ContentTypes = new List<MediaTypeHeaderValue>
                        {
                            new MediaTypeHeaderValue("application/json")
                        }
                    }
                };
            }

            var jwt = _jwtGenerator.Sign(userInformationPayload,
                signedResponseAlg.Value);
            var encryptedResponseAlg = client.GetUserInfoEncryptedResponseAlg();
            var encryptedResponseEnc = client.GetUserInfoEncryptedResponseEnc();
            if (encryptedResponseAlg != null)
            {
                if (encryptedResponseEnc == null)
                {
                    encryptedResponseEnc = JweEnc.A128CBC_HS256;
                }

                jwt = _jwtGenerator.Encrypt(jwt,
                    encryptedResponseAlg.Value,
                    encryptedResponseEnc.Value);
            }

            // Content = new StringContent(jwt, Encoding.UTF8, "application/jwt")
            return new UserInfoResult
            {
                Content = new ContentResult
                {
                    Content = jwt,
                    ContentType = new MediaTypeHeaderValue("application/jwt"),
                    StatusCode = (int)HttpStatusCode.OK
                }
            };
        }
    }
}
