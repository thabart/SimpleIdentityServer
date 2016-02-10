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

        #region Constructors

        public ExternalOpenIdUserAuthenticationAction(
            IAuthenticateHelper authenticateHelper,
            IResourceOwnerRepository resourceOwnerRepository)
        {
            _authenticateHelper = authenticateHelper;
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        #endregion

        #region Public methods

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

            var subjectClaim = claims.FirstOrDefault(r => r.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
            var nameClaim = claims.FirstOrDefault(r => r.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Name);
            var givenNameClaim = claims.FirstOrDefault(r => r.Type == Jwt.Constants.StandardResourceOwnerClaimNames.GivenName);
            var familyNameClaim = claims.FirstOrDefault(r => r.Type == Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName);
            if (subjectClaim == null)
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.NoSubjectCanBeExtracted);
            }

            var resourceOwner = _resourceOwnerRepository.GetBySubject(subjectClaim.Value);
            if (resourceOwner == null)
            {
                resourceOwner = new ResourceOwner
                {
                    Id = subjectClaim.Value,
                    Name = nameClaim == null ? string.Empty : nameClaim.Value,
                    GivenName = givenNameClaim == null ? string.Empty : givenNameClaim.Value,
                    FamilyName = familyNameClaim == null ? string.Empty : familyNameClaim.Value,
                };

                _resourceOwnerRepository.Insert(resourceOwner);
            }

            return _authenticateHelper.ProcessRedirection(authorizationParameter,
                code,
                "subject",
                claims);
        }

        #endregion
    }
}
