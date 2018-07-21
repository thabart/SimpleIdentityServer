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

using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.Account.Actions
{
    public interface IAddResourceOwnerAction
    {
        Task Execute(AddUserParameter addUserParameter);
    }

    public class AddResourceOwnerAction : IAddResourceOwnerAction
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

        public AddResourceOwnerAction(
            IResourceOwnerRepository resourceOwnerRepository,
            IAuthenticateResourceOwnerService authenticateResourceOwnerService)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
        }
        
        public async Task Execute(AddUserParameter addUserParameter)
        {
            if (addUserParameter == null)
            {
                throw new ArgumentNullException(nameof(addUserParameter));
            }

            if (string.IsNullOrEmpty(addUserParameter.Login))
            {
                throw new ArgumentNullException(nameof(addUserParameter.Login));
            }

            if (string.IsNullOrWhiteSpace(addUserParameter.Password))
            {
                throw new ArgumentNullException(nameof(addUserParameter.Password));
            }
            
            if (await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(addUserParameter.Login,
                    addUserParameter.Password).ConfigureAwait(false) != null)
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.UnhandledExceptionCode,
                    Errors.ErrorDescriptions.TheRoWithCredentialsAlreadyExists);
            }

            var claims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()),
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, addUserParameter.Login)
            };
            var newResourceOwner = new ResourceOwner
            {
                Id = addUserParameter.Login,
                Claims = claims,
                TwoFactorAuthentication = TwoFactorAuthentications.NONE,
                IsLocalAccount = true,
                Password = _authenticateResourceOwnerService.GetHashedPassword(addUserParameter.Password)
            };            
            await _resourceOwnerRepository.InsertAsync(newResourceOwner).ConfigureAwait(false);
        }
    }
}
