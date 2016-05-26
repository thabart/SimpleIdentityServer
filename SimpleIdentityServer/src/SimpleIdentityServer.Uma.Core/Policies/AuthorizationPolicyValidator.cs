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

using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Uma.Core.Policies
{
    public interface IAuthorizationPolicyValidator
    {
        AuthorizationPolicyResult IsAuthorized(Ticket validTicket,
               string clientId,
               IEnumerable<System.Security.Claims.Claim> claims);
    }

    internal class AuthorizationPolicyValidator : IAuthorizationPolicyValidator
    {
        private readonly IPolicyRepository _policyRepository;

        private readonly IBasicAuthorizationPolicy _basicAuthorizationPolicy;

        private readonly IResourceSetRepository _resourceSetRepository;

        #region Constructor

        public AuthorizationPolicyValidator(
            IPolicyRepository policyRepository,
            IBasicAuthorizationPolicy basicAuthorizationPolicy,
            IResourceSetRepository resourceSetRepository)
        {
            _policyRepository = policyRepository;
            _basicAuthorizationPolicy = basicAuthorizationPolicy;
            _resourceSetRepository = resourceSetRepository;
        }

        #endregion

        #region Public methods

        public AuthorizationPolicyResult IsAuthorized(Ticket validTicket,
            string clientId,
            IEnumerable<System.Security.Claims.Claim> claims)
        {
            if (validTicket == null)
            {
                throw new ArgumentNullException(nameof(validTicket));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (claims == null || !claims.Any())
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var resourceSet = _resourceSetRepository.GetResourceSetById(validTicket.ResourceSetId);
            if (resourceSet == null)
            {
                throw new BaseUmaException(ErrorCodes.InternalError,
                    string.Format(ErrorDescriptions.TheResourceSetDoesntExist, validTicket.ResourceSetId));
            }

            var authorizationPolicy = _policyRepository.GetPolicy(resourceSet.AuthorizationPolicyId);
            if (authorizationPolicy == null)
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.Authorized
                };
            }

            return _basicAuthorizationPolicy.Execute(validTicket, authorizationPolicy, null);
        }

        #endregion
    }
}
