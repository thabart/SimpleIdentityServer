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

using SimpleIdentityServer.Core.Api.Registration.Actions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Manager.Core.Api.Clients.Actions;
using SimpleIdentityServer.Manager.Core.Parameters;
using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Core.Api.Clients
{
    public interface IClientActions
    {
        List<Client> GetClients();

        Client GetClient(string clientId);

        bool DeleteClient(string clientId);

        bool UpdateClient(UpdateClientParameter updateClientParameter);

        RegistrationResponse AddClient(RegistrationParameter registrationParameter);
    }

    public class ClientActions : IClientActions
    {
        private readonly IGetClientsAction _getClientsAction;

        private readonly IGetClientAction _getClientAction;

        private readonly IRemoveClientAction _removeClientAction;

        private readonly IUpdateClientAction _updateClientAction;

        private readonly IRegisterClientAction _registerClientAction;

        #region Constructor

        public ClientActions(
            IGetClientsAction getClientsAction,
            IGetClientAction getClientAction,
            IRemoveClientAction removeClientAction,
            IUpdateClientAction updateClientAction,
            IRegisterClientAction registerClientAction)
        {
            _getClientsAction = getClientsAction;
            _getClientAction = getClientAction;
            _removeClientAction = removeClientAction;
            _updateClientAction = updateClientAction;
            _registerClientAction = registerClientAction;
        }

        #endregion

        #region Public methods

        public List<Client> GetClients()
        {
            return _getClientsAction.Execute();
        }

        public Client GetClient(string clientId)
        {
            return _getClientAction.Execute(clientId);
        }

        public bool DeleteClient(string clientId)
        {
            return _removeClientAction.Execute(clientId);
        }

        public bool UpdateClient(UpdateClientParameter updateClientParameter)
        {
            return _updateClientAction.Execute(updateClientParameter);
        }

        public RegistrationResponse AddClient(RegistrationParameter registrationParameter)
        {
            return _registerClientAction.Execute(registrationParameter);
        }

        #endregion
    }
}
