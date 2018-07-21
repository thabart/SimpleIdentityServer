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
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface ILoginCallbackAction
    {
        Task<IEnumerable<Claim>> Execute(ClaimsPrincipal claimsPrincipal);
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
        
        public async Task<IEnumerable<Claim>> Execute(ClaimsPrincipal claimsPrincipal)
        {
            // 1. Check parameters.
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            // 2. Check the user is authenticated.
            if (claimsPrincipal.Identity == null ||
                !claimsPrincipal.Identity.IsAuthenticated ||
                !(claimsPrincipal.Identity is ClaimsIdentity))
            {
                throw new IdentityServerException(
                      Errors.ErrorCodes.UnhandledExceptionCode,
                      Errors.ErrorDescriptions.TheUserNeedsToBeAuthenticated);
            }
            
            // 3. Check the subject exists.
            var subject = claimsPrincipal.GetSubject();
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.UnhandledExceptionCode,
                    Errors.ErrorDescriptions.TheRoCannotBeCreated);
            }
            
            // 4. If a user already exists with the same subject then ignore.
            var resourceOwner = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(subject).ConfigureAwait(false);
            if (resourceOwner != null)
            {
                return resourceOwner.Claims;
            }

            // 5. Insert the resource owner.
            var clearPassword = Guid.NewGuid().ToString();
            resourceOwner = new ResourceOwner
            {
                Id = subject,
                IsLocalAccount = false,
                TwoFactorAuthentication = TwoFactorAuthentications.NONE,
                Claims = new List<Claim>(),
                Password = _authenticateResourceOwnerService.GetHashedPassword(clearPassword)
            };
            var claims = await _claimRepository.GetAllAsync().ConfigureAwait(false);
            foreach(var claim in claimsPrincipal.Claims.Where(c => claims.Contains(c.Type)))
            {
                resourceOwner.Claims.Add(claim);
            }

            if (!resourceOwner.Claims.Any(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Subject))
            {
                resourceOwner.Claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject));
            }

            await _resourceOwnerRepository.InsertAsync(resourceOwner).ConfigureAwait(false);
            return resourceOwner.Claims;
        }
    }
}
