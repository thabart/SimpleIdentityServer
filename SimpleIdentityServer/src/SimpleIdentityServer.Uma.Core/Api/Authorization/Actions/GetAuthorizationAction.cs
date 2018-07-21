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

using Newtonsoft.Json;
using SimpleIdentityServer.Uma.Common;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Policies;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Core.Responses;
using SimpleIdentityServer.Uma.Core.Services;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.Authorization.Actions
{
    public interface IGetAuthorizationAction
    {
        Task<AuthorizationResponse> Execute(GetAuthorizationActionParameter parameter, string clientId);
        Task<IEnumerable<AuthorizationResponse>> Execute(IEnumerable<GetAuthorizationActionParameter> parameters, string clientId);
    }

    internal class GetAuthorizationAction : IGetAuthorizationAction
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IAuthorizationPolicyValidator _authorizationPolicyValidator;
        private readonly IConfigurationService _configurationService;
        private readonly IRptRepository _rptRepository;
        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public GetAuthorizationAction(
            ITicketRepository ticketRepository,
            IAuthorizationPolicyValidator authorizationPolicyValidator,
            IConfigurationService configurationService,
            IRptRepository rptRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper,
            IUmaServerEventSource umaServerEventSource)
        {
            _ticketRepository = ticketRepository;
            _authorizationPolicyValidator = authorizationPolicyValidator;
            _configurationService = configurationService;
            _rptRepository = rptRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
            _umaServerEventSource = umaServerEventSource;
        }

        public async Task<AuthorizationResponse> Execute(GetAuthorizationActionParameter getAuthorizationActionParameter, string clientId)
        {
            var result = await Execute(new[] { getAuthorizationActionParameter }, clientId).ConfigureAwait(false);
            return result.First();
        }

        public async Task<IEnumerable<AuthorizationResponse>> Execute(IEnumerable<GetAuthorizationActionParameter> parameters, string clientId)
        {
            var result = new List<AuthorizationResponse>();
            var rptLifeTime = await _configurationService.GetRptLifeTime().ConfigureAwait(false);
            // 1. Check parameters.
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            // 2. Retrieve the tickets.
            _umaServerEventSource.StartGettingAuthorization(JsonConvert.SerializeObject(parameters));
            var tickets = await _ticketRepository.Get(parameters.Select(p => p.TicketId)).ConfigureAwait(false);
            var rptLst = new List<Rpt>();
            // 3. Check parameters.
            foreach(var parameter in parameters)
            {
                var json = JsonConvert.SerializeObject(parameter);
                if (string.IsNullOrWhiteSpace(parameter.TicketId))
                {
                    throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, PostAuthorizationNames.TicketId));
                }

                var ticket = tickets.FirstOrDefault(t => t.Id == parameter.TicketId);
                if (ticket == null)
                {
                    throw new BaseUmaException(ErrorCodes.InvalidTicket,
                        string.Format(ErrorDescriptions.TheTicketDoesntExist, parameter.TicketId));
                }

                if (ticket.ClientId != clientId)
                {
                    throw new BaseUmaException(ErrorCodes.InvalidTicket, ErrorDescriptions.TheTicketIssuerIsDifferentFromTheClient);
                }

                if (ticket.ExpirationDateTime < DateTime.UtcNow)
                {
                    throw new BaseUmaException(ErrorCodes.ExpiredTicket, ErrorDescriptions.TheTicketIsExpired);
                }

                _umaServerEventSource.CheckAuthorizationPolicy(json);
                var authorizationResult = await _authorizationPolicyValidator.IsAuthorized(ticket, clientId, parameter.ClaimTokenParameters).ConfigureAwait(false);
                if (authorizationResult.Type != AuthorizationPolicyResultEnum.Authorized)
                {
                    _umaServerEventSource.RequestIsNotAuthorized(json);
                    result.Add(new AuthorizationResponse
                    {
                        AuthorizationPolicyResult = authorizationResult.Type,
                        ErrorDetails = authorizationResult.ErrorDetails
                    });
                    continue;
                }

                var rpt = new Rpt
                {
                    Value = Guid.NewGuid().ToString(),
                    TicketId = ticket.Id,
                    ResourceSetId = ticket.ResourceSetId,
                    CreateDateTime = DateTime.UtcNow,
                    ExpirationDateTime = DateTime.UtcNow.AddSeconds(rptLifeTime)
                };
                rptLst.Add(rpt);
                result.Add(new AuthorizationResponse
                {
                    AuthorizationPolicyResult = AuthorizationPolicyResultEnum.Authorized,
                    Rpt = rpt.Value
                });
                _umaServerEventSource.RequestIsAuthorized(json);
            }

            // 4. Persist the RPTs.
            if (rptLst.Any())
            {
                await _repositoryExceptionHelper.HandleException(
                    ErrorDescriptions.TheRptCannotBeInserted,
                    () => _rptRepository.Insert(rptLst)).ConfigureAwait(false);
            }

            return result;
        }
    }
}
