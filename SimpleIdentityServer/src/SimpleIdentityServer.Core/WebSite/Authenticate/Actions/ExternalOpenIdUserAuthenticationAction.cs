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
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Services;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface IExternalOpenIdUserAuthenticationAction
    {
        ActionResult Execute(
            List<Claim> claims,
            AuthorizationParameter authorizationParameter,
            string code);
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

        public ActionResult Execute(
            List<Claim> claims,
            AuthorizationParameter authorizationParameter,
            string code)
        {
            if (claims == null || !claims.Any())
            {
                throw new ArgumentNullException("claims");
            }

            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException("code");
            }

            var subject = claims.GetSubject();
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.NoSubjectCanBeExtracted);
            }
            
            var resourceOwner = _authenticateResourceOwnerService.AuthenticateResourceOwner(subject);
            if (resourceOwner == null)
            {
                var standardClaims = _claimRepository.GetAll();
                resourceOwner = new ResourceOwner
                {
                    Id = Guid.NewGuid().ToString(),
                    IsLocalAccount = false,
                    TwoFactorAuthentication = TwoFactorAuthentications.NONE,
                    Claims = claims.Where(c => standardClaims.Any(sc => sc == c.Type)).ToList()
                };                
                _resourceOwnerRepository.Insert(resourceOwner);
            }
            
            return _authenticateHelper.ProcessRedirection(authorizationParameter,
                code,
                "subject",
                claims);
        }
    }
}
