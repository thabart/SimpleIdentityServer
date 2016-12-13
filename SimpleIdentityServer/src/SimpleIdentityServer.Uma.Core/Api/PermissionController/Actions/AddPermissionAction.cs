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
using SimpleIdentityServer.Uma.Core.Configuration;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.PermissionController.Actions
{
    internal interface IAddPermissionAction
    {
        Task<string> Execute(
            AddPermissionParameter addPermissionParameter,
            string clientId);
    }

    internal class AddPermissionAction : IAddPermissionAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;
        private readonly UmaServerOptions _umaServerOptions;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public AddPermissionAction(
            IResourceSetRepository resourceSetRepository,
            ITicketRepository ticketRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper,
            UmaServerOptions umaServerOptions,
            IUmaServerEventSource umaServerEventSource)
        {
            _resourceSetRepository = resourceSetRepository;
            _ticketRepository = ticketRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
            _umaServerOptions = umaServerOptions;
            _umaServerEventSource = umaServerEventSource;
        }

        public async Task<string> Execute(
            AddPermissionParameter addPermissionParameter,
            string clientId)
        {
            var json = addPermissionParameter == null ? string.Empty : JsonConvert.SerializeObject(addPermissionParameter);
            _umaServerEventSource.StartAddPermission(json);
            if (addPermissionParameter == null)
            {
                throw new ArgumentNullException(nameof(addPermissionParameter));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            await CheckAddPermissionParameter(addPermissionParameter);
            var ticketLifetimeInSeconds = _umaServerOptions.TicketLifeTime;
            var ticket = new Ticket
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = clientId,
                ExpirationDateTime = DateTime.UtcNow.AddSeconds(ticketLifetimeInSeconds),
                Scopes = addPermissionParameter.Scopes,
                ResourceSetId = addPermissionParameter.ResourceSetId,
                CreateDateTime = DateTime.UtcNow
            };

            await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheTicketCannotBeInserted, addPermissionParameter.ResourceSetId),
                () => _ticketRepository.Insert(ticket));
            _umaServerEventSource.FinishAddPermission(json);
            return ticket.Id;
        }

        private async Task CheckAddPermissionParameter(AddPermissionParameter addPermissionParameter)
        {
            if (string.IsNullOrWhiteSpace(addPermissionParameter.ResourceSetId))
            {
                throw new BaseUmaException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPermissionNames.ResourceSetId));
            }

            if (addPermissionParameter.Scopes == null ||
                !addPermissionParameter.Scopes.Any())
            {
                throw new BaseUmaException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPermissionNames.Scopes));
            }

            var resourceSet = await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, addPermissionParameter.ResourceSetId),
                () => _resourceSetRepository.Get(addPermissionParameter.ResourceSetId));
            if (resourceSet == null)
            {
                throw new BaseUmaException(
                    ErrorCodes.InvalidResourceSetId,
                    string.Format(ErrorDescriptions.TheResourceSetDoesntExist, addPermissionParameter.ResourceSetId));
            }

            if (resourceSet.Scopes == null ||
                addPermissionParameter.Scopes.Any(s => !resourceSet.Scopes.Contains(s)))
            {
                throw new BaseUmaException(
                    ErrorCodes.InvalidScope,
                    ErrorDescriptions.TheScopeAreNotValid);
            }
        }
    }
}
