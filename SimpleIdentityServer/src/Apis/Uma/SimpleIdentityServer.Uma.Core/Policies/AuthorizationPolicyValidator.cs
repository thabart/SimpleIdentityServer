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
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Policies
{
    public interface IAuthorizationPolicyValidator
    {
        Task<AuthorizationPolicyResult> IsAuthorized(Ticket validTicket, string clientId, ClaimTokenParameter claimTokenParameter);
    }

    internal class AuthorizationPolicyValidator : IAuthorizationPolicyValidator
    {
        private readonly IBasicAuthorizationPolicy _basicAuthorizationPolicy;
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public AuthorizationPolicyValidator(
            IBasicAuthorizationPolicy basicAuthorizationPolicy,
            IResourceSetRepository resourceSetRepository,
            IUmaServerEventSource umaServerEventSource)
        {
            _basicAuthorizationPolicy = basicAuthorizationPolicy;
            _resourceSetRepository = resourceSetRepository;
            _umaServerEventSource = umaServerEventSource;
        }

        #region Public methods

        public async Task<AuthorizationPolicyResult> IsAuthorized(Ticket validTicket, string clientId, ClaimTokenParameter claimTokenParameter)
        {
            if (validTicket == null)
            {
                throw new ArgumentNullException(nameof(validTicket));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }
            
            if (validTicket.Lines == null || !validTicket.Lines.Any())
            {
                throw new ArgumentNullException(nameof(validTicket.Lines));
            }

            var resourceIds = validTicket.Lines.Select(l => l.ResourceSetId);
            var resources = await _resourceSetRepository.Get(resourceIds);
            if (resources == null || !resources.Any() || resources.Count() != resourceIds.Count())
            {
                throw new BaseUmaException(ErrorCodes.InternalError, ErrorDescriptions.SomeResourcesDontExist);
            }

            AuthorizationPolicyResult validationResult = null;
            foreach (var ticketLine in validTicket.Lines)
            {
                var ticketLineParameter = new TicketLineParameter(clientId, ticketLine.Scopes, validTicket.IsAuthorizedByRo);
                var resource = resources.First(r => r.Id == ticketLine.ResourceSetId);
                validationResult = await Validate(ticketLineParameter, resource, claimTokenParameter);
                if (validationResult.Type != AuthorizationPolicyResultEnum.Authorized)
                {
                    _umaServerEventSource.AuthorizationPoliciesFailed(validTicket.Id);
                    return validationResult;
                }
            }

            return validationResult;
        }

        #endregion

        #region Private methods

        private async Task<AuthorizationPolicyResult> Validate(TicketLineParameter ticketLineParameter, ResourceSet resource, ClaimTokenParameter claimTokenParameter)
        {
            if (resource.Policies == null || !resource.Policies.Any())
            {
                return new AuthorizationPolicyResult
                {
                    Type = AuthorizationPolicyResultEnum.Authorized
                };
            }
            
            foreach (var authorizationPolicy in resource.Policies)
            {
                var result = await _basicAuthorizationPolicy.Execute(ticketLineParameter, authorizationPolicy, claimTokenParameter);
                if (result.Type == AuthorizationPolicyResultEnum.Authorized)
                {
                    return result;
                }
            }

            return new AuthorizationPolicyResult
            {
                Type = AuthorizationPolicyResultEnum.NotAuthorized
            };
        }

        #endregion
    }
}
