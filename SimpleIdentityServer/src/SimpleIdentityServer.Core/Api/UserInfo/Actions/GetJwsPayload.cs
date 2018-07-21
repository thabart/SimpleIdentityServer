﻿#region copyright
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
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;
using System;
using System.Buffers;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.UserInfo.Actions
{
    public interface IGetJwsPayload
    {
        Task<UserInfoResult> Execute(string accessToken);
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

        public async Task<UserInfoResult> Execute(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            // Check if the access token is still valid otherwise raise an authorization exception.
            GrantedTokenValidationResult valResult;
            if (!((valResult = await _grantedTokenValidator.CheckAccessTokenAsync(accessToken).ConfigureAwait(false)).IsValid))
            {
                throw new AuthorizationException(valResult.MessageErrorCode, valResult.MessageErrorDescription);
            }

            var grantedToken = await _grantedTokenRepository.GetTokenAsync(accessToken).ConfigureAwait(false);
            var client = await _clientRepository.GetClientByIdAsync(grantedToken.ClientId).ConfigureAwait(false);
            if (client == null)
            {
                client = await _clientRepository.GetClientByIdAsync(Constants.AnonymousClientId).ConfigureAwait(false);
                if (client == null)
                {
                    throw new IdentityServerException(ErrorCodes.InternalError,
                        string.Format(ErrorDescriptions.ClientIsNotValid, Constants.AnonymousClientId));
                }
            }

            var signedResponseAlg = client.GetUserInfoSignedResponseAlg();
            var userInformationPayload = grantedToken.UserInfoPayLoad;
            if (signedResponseAlg == null ||
                signedResponseAlg.Value == JwsAlg.none)
            {
                var objectResult = new ObjectResult(grantedToken.UserInfoPayLoad)
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
                objectResult.ContentTypes.Add(new MediaTypeHeaderValue("application/json"));
                objectResult.Formatters.Add(new JsonOutputFormatter(new JsonSerializerSettings(), ArrayPool<char>.Shared));
                return new UserInfoResult
                {
                    Content = objectResult
                };
            }

            var jwt = await _jwtGenerator.SignAsync(userInformationPayload,
                signedResponseAlg.Value).ConfigureAwait(false);
            var encryptedResponseAlg = client.GetUserInfoEncryptedResponseAlg();
            var encryptedResponseEnc = client.GetUserInfoEncryptedResponseEnc();
            if (encryptedResponseAlg != null)
            {
                if (encryptedResponseEnc == null)
                {
                    encryptedResponseEnc = JweEnc.A128CBC_HS256;
                }

                jwt = await _jwtGenerator.EncryptAsync(jwt,
                    encryptedResponseAlg.Value,
                    encryptedResponseEnc.Value).ConfigureAwait(false);
            }

            // Content = new StringContent(jwt, Encoding.UTF8, "application/jwt")
            return new UserInfoResult
            {
                Content = new ContentResult
                {
                    Content = jwt,
                    StatusCode = (int)HttpStatusCode.OK,
                    ContentType = "application/jwt",
                }
            };
        }
    }
}
