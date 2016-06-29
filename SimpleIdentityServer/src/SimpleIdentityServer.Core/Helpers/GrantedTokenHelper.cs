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

using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using System;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IGrantedTokenHelper 
    {
        GrantedToken GetValidGrantedToken(
            string scopes,
            string clientId,
            JwsPayload idTokenJwsPayload = null,
            JwsPayload userInfoJwsPayload = null);
    }

    internal class GrantedTokenHelper : IGrantedTokenHelper
    {
        private readonly IGrantedTokenRepository _grantedTokenRepository;

        private readonly IGrantedTokenValidator _grantedTokenValidator;

        #region Constructor

        public GrantedTokenHelper(
            IGrantedTokenRepository grantedTokenRepository,
            IGrantedTokenValidator grantedTokenValidator)
        {
            _grantedTokenRepository = grantedTokenRepository;
            _grantedTokenValidator = grantedTokenValidator;
        }

        #endregion

        #region Public methods

        public GrantedToken GetValidGrantedToken(
            string scopes,
            string clientId,
            JwsPayload idTokenJwsPayload = null,
            JwsPayload userInfoJwsPayload = null)
        {
            if (string.IsNullOrWhiteSpace(scopes))
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var token = _grantedTokenRepository.GetToken(
                scopes,
                clientId,
                idTokenJwsPayload,
                userInfoJwsPayload);
            if (token == null)
            {
                return null;
            }

            string messageErrorCode;
            string messageErrorDescription;
            if (!_grantedTokenValidator.CheckAccessToken(token.AccessToken, out messageErrorCode, out messageErrorDescription))
            {
                return null;
            }

            return token;
        }

        #endregion
    }
}
