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

using SimpleIdentityServer.Uma.Core.Configuration;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Policies;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Core.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdentityServer.Uma.Core.Api.Authorization.Actions
{
    public interface IGetAuthorizationAction
    {
        AuthorizationResponse Execute(
               GetAuthorizationActionParameter getAuthorizationActionParameter,
               IEnumerable<Claim> claims);
    }

    internal class GetAuthorizationAction : IGetAuthorizationAction
    {
        private readonly ITicketRepository _ticketRepository;

        private readonly IAuthorizationPolicyValidator _authorizationPolicyValidator;

        private readonly IUmaServerConfigurationProvider _umaServerConfigurationProvider;

        private readonly IRptRepository _rptRepository;

        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;

        #region Constructor

        public GetAuthorizationAction(
            ITicketRepository ticketRepository,
            IAuthorizationPolicyValidator authorizationPolicyValidator,
            IUmaServerConfigurationProvider umaServerConfigurationProvider,
            IRptRepository rptRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper)
        {
            _ticketRepository = ticketRepository;
            _authorizationPolicyValidator = authorizationPolicyValidator;
            _umaServerConfigurationProvider = umaServerConfigurationProvider;
            _rptRepository = rptRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
        }

        #endregion

        #region Public methods

        public AuthorizationResponse Execute(
            GetAuthorizationActionParameter getAuthorizationActionParameter,
            IEnumerable<Claim> claims)
        {
            if (getAuthorizationActionParameter == null)
            {
                throw new ArgumentNullException(nameof(getAuthorizationActionParameter));
            }

            if (claims == null ||
                !claims.Any())
            {
                throw new ArgumentNullException(nameof(claims));
            }

            if (string.IsNullOrWhiteSpace(getAuthorizationActionParameter.TicketId))
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "ticket_id"));
            }

            var ticket = _ticketRepository.GetTicketById(getAuthorizationActionParameter.TicketId);
            if (ticket == null)
            {
                throw new BaseUmaException(ErrorCodes.InvalidTicket,
                    string.Format(ErrorDescriptions.TheTicketDoesntExist, getAuthorizationActionParameter.TicketId));
            }

            var clientId = GetClientId(claims);
            if (ticket.ClientId != clientId)
            {
                throw new BaseUmaException(ErrorCodes.InvalidTicket,
                    ErrorDescriptions.TheTicketIssuerIsDifferentFromTheClient);
            }

            if (ticket.ExpirationDateTime < DateTime.UtcNow)
            {
                throw new BaseUmaException(ErrorCodes.ExpiredTicket,
                    ErrorDescriptions.TheTicketIsExpired);
            }

            var isAuthorized = _authorizationPolicyValidator.IsAuthorized(ticket,
                clientId,
                claims);
            if (isAuthorized != AuthorizationPolicyResultEnum.Authorized)
            {
                return new AuthorizationResponse
                {
                    AuthorizationPolicyResult = isAuthorized
                };
            }

            var rpt = new Rpt
            {
                ExpirationDateTime = DateTime.UtcNow.AddSeconds(_umaServerConfigurationProvider.GetRptLifetime()),
                Value = Guid.NewGuid().ToString(),
                TicketId = ticket.Id,
                ResourceSetId = ticket.ResourceSetId
            };

            _repositoryExceptionHelper.HandleException(
                ErrorDescriptions.TheRptCannotBeInserted,
                () => _rptRepository.InsertRpt(rpt));
            return new AuthorizationResponse
            {
                AuthorizationPolicyResult = AuthorizationPolicyResultEnum.Authorized,
                Rpt = rpt.Value
            };
        }

        #endregion

        #region Private static methods

        private static string GetClientId(IEnumerable<Claim> claims)
        {
            var clientClaim = claims.FirstOrDefault(c => c.Type == "client_id");
            if (clientClaim == null)
            {
                return string.Empty;
            }

            return clientClaim.Value;
        }

        #endregion
    }
}
