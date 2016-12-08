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
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Models;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IGrantedTokenValidator
    {
        Task<GrantedTokenValidationResult> CheckAccessTokenAsync(string accessToken);
        Task<GrantedTokenValidationResult> CheckRefreshTokenAsync(string refreshToken);
        GrantedTokenValidationResult CheckGrantedToken(GrantedToken token);
    }

    public class GrantedTokenValidationResult
    {
        public bool IsValid { get; set; }
        public string MessageErrorCode { get; set; }
        public string MessageErrorDescription { get; set; }
    }

    public class GrantedTokenValidator : IGrantedTokenValidator
    {
        private readonly IGrantedTokenRepository _grantedTokenRepository;

        public GrantedTokenValidator(IGrantedTokenRepository grantedTokenRepository)
        {
            _grantedTokenRepository = grantedTokenRepository;
        }

        public async Task<GrantedTokenValidationResult> CheckAccessTokenAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var grantedToken = await _grantedTokenRepository.GetTokenAsync(accessToken);
            return CheckGrantedToken(grantedToken);
        }

        public async Task<GrantedTokenValidationResult> CheckRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            var grantedToken = await _grantedTokenRepository.GetTokenByRefreshTokenAsync(refreshToken);
            return CheckGrantedToken(grantedToken);
        }

        public GrantedTokenValidationResult CheckGrantedToken(GrantedToken grantedToken)
        {
            if (grantedToken == null)
            {
                return new GrantedTokenValidationResult
                {
                    MessageErrorCode = ErrorCodes.InvalidToken,
                    MessageErrorDescription = ErrorDescriptions.TheTokenIsNotValid,
                    IsValid = false
                };
            }

            var expirationDateTime = grantedToken.CreateDateTime.AddSeconds(grantedToken.ExpiresIn);
            var tokenIsExpired = DateTime.UtcNow > expirationDateTime;
            if (tokenIsExpired)
            {
                return new GrantedTokenValidationResult
                {
                    MessageErrorCode = ErrorCodes.InvalidToken,
                    MessageErrorDescription = ErrorDescriptions.TheTokenIsExpired,
                    IsValid = false
                };
            }

            return new GrantedTokenValidationResult
            {
                IsValid = true
            };
        }
    }
}
