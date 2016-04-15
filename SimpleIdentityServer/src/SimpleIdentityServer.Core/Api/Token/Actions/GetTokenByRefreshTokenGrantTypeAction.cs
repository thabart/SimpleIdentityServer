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

using System;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IGetTokenByRefreshTokenGrantTypeAction
    {
        GrantedToken Execute(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter);
    }

    public sealed class GetTokenByRefreshTokenGrantTypeAction : IGetTokenByRefreshTokenGrantTypeAction
    {
        private readonly IGrantedTokenRepository _grantedTokenRepository;

        private readonly IClientHelper _clientHelper;

        public GetTokenByRefreshTokenGrantTypeAction(
            IGrantedTokenRepository grantedTokenRepository,
            IClientHelper clientHelper)
        {
            _grantedTokenRepository = grantedTokenRepository;
            _clientHelper = clientHelper;
        }

        public GrantedToken Execute(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter)
        {
            if (refreshTokenGrantTypeParameter == null)
            {
                throw new ArgumentNullException("refreshTokenGrantTypeParameter");
            }

            var grantedToken = ValidateParameter(refreshTokenGrantTypeParameter);
            if (grantedToken.IdTokenPayLoad != null)
            {
                grantedToken.IdToken = _clientHelper.GenerateIdToken(
                    grantedToken.ClientId,
                    grantedToken.IdTokenPayLoad);
            }

            return grantedToken;
        }

        private GrantedToken ValidateParameter(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter)
        {
            var grantedToken = _grantedTokenRepository.GetTokenByRefreshToken(refreshTokenGrantTypeParameter.RefreshToken);
            if (grantedToken == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheRefreshTokenIsNotValid);
            }

            return grantedToken;
        }
    }
}
