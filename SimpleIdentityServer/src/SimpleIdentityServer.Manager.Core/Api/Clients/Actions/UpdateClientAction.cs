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

using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Manager.Core.Api.Clients.Actions
{
    public interface IUpdateClientAction
    {
        bool Execute(UpdateClientParameter updateClientParameter);
    }
    
    public class UpdateClientAction : IUpdateClientAction
    {
        private readonly IClientRepository _clientRepository;

        private readonly IGenerateClientFromRegistrationRequest _generateClientFromRegistrationRequest;

        #region Constructor

        public UpdateClientAction(
            IClientRepository clientRepository,
            IGenerateClientFromRegistrationRequest generateClientFromRegistrationRequest)
        {
            _clientRepository = clientRepository;
            _generateClientFromRegistrationRequest = generateClientFromRegistrationRequest;
        }

        #endregion

        #region Public methods

        public bool Execute(UpdateClientParameter updateClientParameter)
        {
            if (updateClientParameter == null)
            {
                throw new ArgumentNullException(nameof(updateClientParameter));
            }

            var existedClient = _clientRepository.GetClientById(updateClientParameter.ClientId);
            if (existedClient == null)
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidParameterCode,
                    string.Format(ErrorDescriptions.TheClientDoesntExist, updateClientParameter.ClientId));
            }

            SimpleIdentityServer.Core.Models.Client client = null;
            try
            {
                client = _generateClientFromRegistrationRequest.Execute(updateClientParameter);
            }
            catch(IdentityServerException ex)
            {
                throw new IdentityServerManagerException(ex.Code, ex.Message);
            }

            client.ClientId = existedClient.ClientId;
            client.AllowedScopes = updateClientParameter.AllowedScopes == null 
                ? new List<Scope>() 
                : updateClientParameter.AllowedScopes.Select(s => new Scope
            {
                Name = s
            }).ToList();
            return _clientRepository.UpdateClient(client);
        }

        #endregion
    }
}
