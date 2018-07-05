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

using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Store;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface IGenerateAndSendCodeAction
    {
        Task<string> ExecuteAsync(string subject);
    }

    internal class GenerateAndSendCodeAction : IGenerateAndSendCodeAction
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IConfirmationCodeStore _confirmationCodeStore;
        private readonly ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandler;

        public GenerateAndSendCodeAction(
            IResourceOwnerRepository resourceOwnerRepository,
            IConfirmationCodeStore confirmationCodeStore,
            ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _confirmationCodeStore = confirmationCodeStore;
            _twoFactorAuthenticationHandler = twoFactorAuthenticationHandler;
        }
        

        public async Task<string> ExecuteAsync(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }
            
            var resourceOwner = await _resourceOwnerRepository.GetAsync(subject);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, ErrorDescriptions.TheRoDoesntExist);
            }

            if (string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
            {
                throw new IdentityServerException(
                    ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.TwoFactorAuthenticationIsNotEnabled);
            }

            var confirmationCode = new ConfirmationCode
            {
                Value = await GetCode(),
                IssueAt = DateTime.UtcNow,
                ExpiresIn = 300
            };

            var service = _twoFactorAuthenticationHandler.Get(resourceOwner.TwoFactorAuthentication);
            if (!resourceOwner.Claims.Any(c => c.Type == service.RequiredClaim))
            {
                throw new ClaimRequiredException(service.RequiredClaim);
            }

            if (!await _confirmationCodeStore.Add(confirmationCode))
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode, ErrorDescriptions.TheConfirmationCodeCannotBeSaved);
            }

            await _twoFactorAuthenticationHandler.SendCode(confirmationCode.Value, resourceOwner.TwoFactorAuthentication, resourceOwner);
            return confirmationCode.Value;
        }
        
        private async Task<string> GetCode()
        {
            var random = new Random();
            var number = random.Next(100000, 999999);
            if (await _confirmationCodeStore.Get(number.ToString()) != null)
            {
                return await GetCode();
            }

            return number.ToString();
        }
    }
}
