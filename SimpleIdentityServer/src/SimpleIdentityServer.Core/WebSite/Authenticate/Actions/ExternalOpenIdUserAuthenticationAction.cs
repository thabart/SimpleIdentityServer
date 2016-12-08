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
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface IExternalOpenIdUserAuthenticationAction
    {
        Task<ExternalOpenIdAuthenticationResult> Execute(
            List<Claim> claims,
            AuthorizationParameter authorizationParameter,
            string code);
    }

    public class ExternalOpenIdAuthenticationResult
    {
        public ActionResult ActionResult { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }

    public sealed class ExternalOpenIdUserAuthenticationAction : IExternalOpenIdUserAuthenticationAction
    {
        private readonly IAuthenticateHelper _authenticateHelper;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;
        private readonly IClaimRepository _claimRepository;

        public ExternalOpenIdUserAuthenticationAction(
            IAuthenticateHelper authenticateHelper,
            IResourceOwnerRepository resourceOwnerRepository,
            IAuthenticateResourceOwnerService authenticateResourceOwnerService,
            IClaimRepository claimRepository)
        {
            _authenticateHelper = authenticateHelper;
            _resourceOwnerRepository = resourceOwnerRepository;
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
            _claimRepository = claimRepository;
        }

        public async Task<ExternalOpenIdAuthenticationResult> Execute(
            List<Claim> claims,
            AuthorizationParameter authorizationParameter,
            string code)
        {
            // 1. Check parameters.
            if (claims == null || !claims.Any())
            {
                throw new ArgumentNullException(nameof(claims));
            }

            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            // 2. Subject cannot be extracted.
            var subject = claims.GetSubject();
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.NoSubjectCanBeExtracted);
            }
            
            // 3. Create the resource owner if needed.
            var resourceOwner = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(subject);
            if (resourceOwner == null)
            {
                var standardClaims = await _claimRepository.GetAllAsync();
                resourceOwner = new ResourceOwner
                {
                    Id = Guid.NewGuid().ToString(),
                    IsLocalAccount = false,
                    TwoFactorAuthentication = TwoFactorAuthentications.NONE,
                    Claims = claims.Where(c => standardClaims.Any(sc => sc == c.Type)).ToList()
                };
                if (!resourceOwner.Claims.Any(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Subject))
                {
                    resourceOwner.Claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject));
                }

                await _resourceOwnerRepository.InsertAsync(resourceOwner);
            }
            return new ExternalOpenIdAuthenticationResult
            {
                ActionResult = _authenticateHelper.ProcessRedirection(authorizationParameter,
                                code,
                                "subject",
                                claims),
                Claims = resourceOwner.Claims
            };
        }
    }
}
