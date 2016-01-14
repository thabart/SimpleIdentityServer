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
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using System;

namespace SimpleIdentityServer.Core.Api.UserInfo.Actions
{
    public interface IGetJwsPayload
    {
        JwsPayload Execute(string accessToken);
    }

    public class GetJwsPayload : IGetJwsPayload
    {
        private readonly IGrantedTokenValidator _grantedTokenValidator;

        private readonly IGrantedTokenRepository _grantedTokenRepository;

        private readonly IJwtParser _jwtParser;

        private readonly IClientRepository _clientRepository;

        public GetJwsPayload(
            IGrantedTokenValidator grantedTokenValidator,
            IGrantedTokenRepository grantedTokenRepository,
            IJwtParser jwtParser,
            IClientRepository clientRepository)
        {
            _grantedTokenValidator = grantedTokenValidator;
            _grantedTokenRepository = grantedTokenRepository;
            _jwtParser = jwtParser;
            _clientRepository = clientRepository;
        }

        public JwsPayload Execute(string accessToken)
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
            if (signedResponseAlg == null ||
                signedResponseAlg.Value == JwsAlg.none)
            {
                return grantedToken.UserInfoPayLoad;
            }

            return null;
        }
    }
}
