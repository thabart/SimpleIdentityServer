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

using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using System;
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
        private readonly IConfirmationCodeRepository _confirmationCodeRepository;
        private readonly ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandler;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

        public GenerateAndSendCodeAction(
            IResourceOwnerRepository resourceOwnerRepository,
            IConfirmationCodeRepository confirmationCodeRepository,
            ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
            IAuthenticateResourceOwnerService authenticateResourceOwnerService)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _confirmationCodeRepository = confirmationCodeRepository;
            _twoFactorAuthenticationHandler = twoFactorAuthenticationHandler;
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
        }
        

        public async Task<string> ExecuteAsync(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }
            
            var resourceOwner = _authenticateResourceOwnerService.AuthenticateResourceOwner(subject);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.TheRoDoesntExist);
            }

            if (resourceOwner.TwoFactorAuthentication ==  TwoFactorAuthentications.NONE)
            {
                throw new IdentityServerException(
                    ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.TwoFactorAuthenticationIsNotEnabled);
            }

            var confirmationCode = new ConfirmationCode
            {
                Code = GetCode(),
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = 300,
                IsConfirmed = false
            };

            if (!_confirmationCodeRepository.AddCode(confirmationCode))
            {
                throw new IdentityServerException(
                    ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.TheConfirmationCodeCannotBeSaved);
            }

            await _twoFactorAuthenticationHandler.SendCode(confirmationCode.Code, (int)resourceOwner.TwoFactorAuthentication, resourceOwner);
            return confirmationCode.Code;
        }
        
        private string GetCode()
        {
            var random = new Random();
            var number = random.Next(100000, 999999);
            if (_confirmationCodeRepository.Get(number.ToString()) != null)
            {
                return GetCode();
            }

            return number.ToString();
        }
    }
}
