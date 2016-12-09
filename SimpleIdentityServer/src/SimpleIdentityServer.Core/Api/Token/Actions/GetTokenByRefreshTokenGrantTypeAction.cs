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
using SimpleIdentityServer.Logging;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IGetTokenByRefreshTokenGrantTypeAction
    {
        Task<GrantedToken> Execute(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter);
    }

    public sealed class GetTokenByRefreshTokenGrantTypeAction : IGetTokenByRefreshTokenGrantTypeAction
    {
        private readonly IGrantedTokenRepository _grantedTokenRepository;
        private readonly IClientHelper _clientHelper;
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;

        public GetTokenByRefreshTokenGrantTypeAction(
            IGrantedTokenRepository grantedTokenRepository,
            IClientHelper clientHelper,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper)
        {
            _grantedTokenRepository = grantedTokenRepository;
            _clientHelper = clientHelper;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
        }

        public async Task<GrantedToken> Execute(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter)
        {
            if (refreshTokenGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(refreshTokenGrantTypeParameter));
            }

            // 1. Validate parameters
            var grantedToken = await ValidateParameter(refreshTokenGrantTypeParameter);
            // 2. Generate a new access token & insert it
            var generatedToken = await _grantedTokenGeneratorHelper.GenerateTokenAsync(
                grantedToken.ClientId,
                grantedToken.Scope,
                grantedToken.UserInfoPayLoad,
                grantedToken.IdTokenPayLoad);
            generatedToken.ParentTokenId = grantedToken.Id;
            await _grantedTokenRepository.InsertAsync(generatedToken);
            // 3. Fill-in the idtoken
            if (generatedToken.IdTokenPayLoad != null)
            {
                generatedToken.IdToken = await _clientHelper.GenerateIdTokenAsync(generatedToken.ClientId, generatedToken.IdTokenPayLoad);
            }

            _simpleIdentityServerEventSource.GrantAccessToClient(generatedToken.ClientId,
                generatedToken.AccessToken,
                generatedToken.Scope);
            return generatedToken;
        }

        private async Task<GrantedToken> ValidateParameter(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter)
        {
            // 1. Check refresh token exists
            var grantedToken = await _grantedTokenRepository.GetTokenByRefreshTokenAsync(refreshTokenGrantTypeParameter.RefreshToken);
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
