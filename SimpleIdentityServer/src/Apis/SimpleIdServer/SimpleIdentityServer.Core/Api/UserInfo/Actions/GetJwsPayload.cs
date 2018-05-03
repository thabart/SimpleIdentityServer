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
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Stores;
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
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IClientRepository _clientRepository;
        private readonly ITokenStore _tokenStore;

        public GetJwsPayload(
            IGrantedTokenValidator grantedTokenValidator,
            IJwtGenerator jwtGenerator,
            IClientRepository clientRepository,
            ITokenStore tokenStore)
        {
            _grantedTokenValidator = grantedTokenValidator;
            _jwtGenerator = jwtGenerator;
            _clientRepository = clientRepository;
            _tokenStore = tokenStore;
        }

        public async Task<UserInfoResult> Execute(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            // Check if the access token is still valid otherwise raise an authorization exception.
            GrantedTokenValidationResult valResult;
            if (!((valResult = await _grantedTokenValidator.CheckAccessTokenAsync(accessToken)).IsValid))
            {
                throw new AuthorizationException(valResult.MessageErrorCode, valResult.MessageErrorDescription);
            }

            var grantedToken = await _tokenStore.GetAccessToken(accessToken);
            var client = await _clientRepository.GetClientByIdAsync(grantedToken.ClientId);
            if (client == null)
            {
                client = await _clientRepository.GetClientByIdAsync(Constants.AnonymousClientId);
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
                signedResponseAlg.Value);
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
                    encryptedResponseEnc.Value);
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
