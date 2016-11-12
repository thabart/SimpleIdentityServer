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
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface ILoginCallbackAction
    {
        void Execute(ClaimsPrincipal claimsPrincipal);
    }

    internal class LoginCallbackAction : ILoginCallbackAction
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;


        public LoginCallbackAction(
            IResourceOwnerRepository resourceOwnerRepository,
            IClaimRepository claimRepository,
            IAuthenticateResourceOwnerService authenticateResourceOwnerService)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _claimRepository = claimRepository;
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
        }
        
        public void Execute(ClaimsPrincipal claimsPrincipal)
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
                    Errors.ErrorDescriptions.TheRoCannotBeCreated);
            }

            var resourceOwner = _authenticateResourceOwnerService.AuthenticateResourceOwner(subject);
            if (resourceOwner != null)
            {
                return;
            }

            var clearPassword = Guid.NewGuid().ToString();
            resourceOwner = new ResourceOwner
            {
                Id = Guid.NewGuid().ToString(),
                IsLocalAccount = false,
                TwoFactorAuthentication = TwoFactorAuthentications.NONE,
                Claims = new List<Claim>(),
                Password = _authenticateResourceOwnerService.GetHashedPassword(clearPassword)
            };

            var claims = _claimRepository.GetAll();
            foreach(var claim in claimsPrincipal.Claims.Where(c => claims.Contains(c.Type)))
            {
                resourceOwner.Claims.Add(claim);
            }

            _resourceOwnerRepository.Insert(resourceOwner);
        }
    }
}
