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
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Linq;

namespace SimpleIdentityServer.Uma.Core.Api.PermissionController.Actions
{
    internal interface IAddPermissionAction
    {
        string Execute(
            AddPermissionParameter addPermissionParameter,
            string clientId);
    }

    internal class AddPermissionAction : IAddPermissionAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        private readonly ITicketRepository _ticketRepository;

        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;

        private readonly IUmaServerConfigurationProvider _umaServerConfigurationProvider;

        #region Constructor

        public AddPermissionAction(
            IResourceSetRepository resourceSetRepository,
            ITicketRepository ticketRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper,
            IUmaServerConfigurationProvider umaServerConfigurationProvider)
        {
            _resourceSetRepository = resourceSetRepository;
            _ticketRepository = ticketRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
            _umaServerConfigurationProvider = umaServerConfigurationProvider;
        }

        #endregion

        #region Public methods

        public string Execute(
            AddPermissionParameter addPermissionParameter,
            string clientId)
        {
            if (addPermissionParameter == null)
            {
                throw new ArgumentNullException(nameof(addPermissionParameter));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            CheckAddPermissionParameter(addPermissionParameter);
            var ticketLifetimeInSeconds = _umaServerConfigurationProvider.GetTicketLifetime();
            var ticket = new Ticket
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = clientId,
                ExpirationDateTime = DateTime.UtcNow.AddSeconds(ticketLifetimeInSeconds),
                Scopes = addPermissionParameter.Scopes,
                ResourceSetId = addPermissionParameter.ResourceSetId
            };

            var newTicket = _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheTicketCannotBeInserted, addPermissionParameter.ResourceSetId),
                () => _ticketRepository.InsertTicket(ticket));
            return newTicket.Id;
        }

        #endregion

        #region Private methods

        private void CheckAddPermissionParameter(AddPermissionParameter addPermissionParameter)
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

            var resourceSet = _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, addPermissionParameter.ResourceSetId),
                () => _resourceSetRepository.GetResourceSetById(addPermissionParameter.ResourceSetId));
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

        #endregion
    }
}
