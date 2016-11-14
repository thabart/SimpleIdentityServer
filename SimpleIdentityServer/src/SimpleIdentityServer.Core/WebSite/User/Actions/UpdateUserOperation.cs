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
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using System;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    public interface IUpdateUserOperation
    {
        bool Execute(UpdateUserParameter updateUserParameter);
    }

    internal class UpdateUserOperation : IUpdateUserOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

        public UpdateUserOperation(
            IResourceOwnerRepository resourceOwnerRepository,
            IAuthenticateResourceOwnerService authenticateResourceOwnerService)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
        }
        
        public bool Execute(UpdateUserParameter updateUserParameter)
        {
            if (updateUserParameter == null)
            {
                throw new ArgumentNullException(nameof(updateUserParameter));
            }

            if (string.IsNullOrWhiteSpace(updateUserParameter.Login))
            {
                throw new ArgumentNullException(nameof(updateUserParameter.Login));
            }

            var resourceOwner = _authenticateResourceOwnerService.AuthenticateResourceOwner(updateUserParameter.Login);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.InternalError,
                    Errors.ErrorDescriptions.TheRoDoesntExist);
            }
            
            resourceOwner.TwoFactorAuthentication = updateUserParameter.TwoFactorAuthentication;
            if (!string.IsNullOrWhiteSpace(updateUserParameter.Password))
            {
                resourceOwner.Password = _authenticateResourceOwnerService.GetHashedPassword(updateUserParameter.Password);
            }

            resourceOwner.Claims = updateUserParameter.Claims;
            if (resourceOwner.Claims != null)
            {
                Claim updatedClaim;
                if (((updatedClaim = resourceOwner.Claims.FirstOrDefault(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt)) != null))
                {
                    resourceOwner.Claims.Remove(updatedClaim);
                }

                resourceOwner.Claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()));
            }

            return _resourceOwnerRepository.Update(resourceOwner);
        }
    }
}
