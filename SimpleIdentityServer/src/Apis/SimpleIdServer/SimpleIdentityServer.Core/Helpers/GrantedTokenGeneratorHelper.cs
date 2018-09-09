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

using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Services;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IGrantedTokenGeneratorHelper
    {
        Task<GrantedToken> GenerateTokenAsync(string clientId, string scope, JwsPayload userInformationPayload = null, JwsPayload idTokenPayload = null);
        Task<GrantedToken> GenerateTokenAsync(Core.Common.Models.Client clientId, string scope, JwsPayload userInformationPayload = null, JwsPayload idTokenPayload = null);
    }

    public class GrantedTokenGeneratorHelper : IGrantedTokenGeneratorHelper
    {
        private readonly IConfigurationService _configurationService;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IClientHelper _clientHelper;
        private readonly IClientRepository _clientRepository;

        public GrantedTokenGeneratorHelper(IConfigurationService configurationService,
            IJwtGenerator jwtGenerator, IClientHelper clientHelper, IClientRepository clientRepository)
        {
            _configurationService = configurationService;
            _jwtGenerator = jwtGenerator;
            _clientHelper = clientHelper;
            _clientRepository = clientRepository;
        }

        public async Task<GrantedToken> GenerateTokenAsync(string clientId, string scope, JwsPayload userInformationPayload = null, JwsPayload idTokenPayload = null)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var client = await _clientRepository.GetClientByIdAsync(clientId).ConfigureAwait(false);
            if (client == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient, ErrorDescriptions.TheClientIdDoesntExist);
            }

            return await GenerateTokenAsync(client, scope, userInformationPayload, idTokenPayload).ConfigureAwait(false);
        }

        public async Task<GrantedToken> GenerateTokenAsync(Core.Common.Models.Client client, string scope, JwsPayload userInformationPayload = null, JwsPayload idTokenPayload = null)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var expiresIn = (int) await _configurationService.GetTokenValidityPeriodInSecondsAsync().ConfigureAwait(false); // 1. Retrieve the expiration time of the granted token.
            var jwsPayload = await _jwtGenerator.GenerateAccessToken(client, scope.Split(' ')).ConfigureAwait(false); // 2. Construct the JWT token (client).
            var accessToken = await _clientHelper.GenerateIdTokenAsync(client, jwsPayload).ConfigureAwait(false);
            var refreshTokenId = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()); // 3. Construct the refresh token.
            return new GrantedToken
            {
                AccessToken = accessToken,
                RefreshToken = Convert.ToBase64String(refreshTokenId),
                ExpiresIn = expiresIn,
                TokenType = Constants.StandardTokenTypes.Bearer,
                CreateDateTime = DateTime.UtcNow,
                // IDS
                Scope = scope,
                UserInfoPayLoad = userInformationPayload,
                IdTokenPayLoad = idTokenPayload,
                ClientId = client.ClientId
            };
        }
    }
}
