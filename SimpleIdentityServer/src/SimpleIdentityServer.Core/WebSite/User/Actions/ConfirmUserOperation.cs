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
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    public interface IConfirmUserOperation
    {
        Task Execute(ClaimsPrincipal claimsPrincipal);
    }

    internal class ConfirmUserOperation : IConfirmUserOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

        public ConfirmUserOperation(
            IResourceOwnerRepository resourceOwnerRepository,
            IAuthenticateResourceOwnerService authenticateResourceOwnerService)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
        }
        
        public async Task Execute(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            if (claimsPrincipal.Identity == null ||
                !claimsPrincipal.Identity.IsAuthenticated ||
                !(claimsPrincipal.Identity is ClaimsIdentity))
            {
                throw new IdentityServerException(
                      Errors.ErrorCodes.UnhandledExceptionCode,
                      Errors.ErrorDescriptions.TheUserNeedsToBeAuthenticated);
            }

            var subject = claimsPrincipal.GetSubject();
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.UnhandledExceptionCode,
                    Errors.ErrorDescriptions.TheSubjectCannotBeRetrieved);
            }
            
            var result = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(subject).ConfigureAwait(false);
            if (result == null)
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.UnhandledExceptionCode,
                    Errors.ErrorDescriptions.TheRoDoesntExist);
            }

            if (result.IsLocalAccount)
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.UnhandledExceptionCode,
                    Errors.ErrorDescriptions.TheAccountHasAlreadyBeenActivated);
            }

            result.IsLocalAccount = true;
            if (result.Claims != null)
            {
                Claim updatedClaim;
                if (((updatedClaim = result.Claims.FirstOrDefault(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt)) != null))
                {
                    result.Claims.Remove(updatedClaim);
                }

                result.Claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()));
            }

            await _resourceOwnerRepository.UpdateAsync(result).ConfigureAwait(false);
        }
    }
}
