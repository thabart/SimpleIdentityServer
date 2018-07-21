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

using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Clients.Actions
{
    public interface IRemoveClientAction
    {
        Task<bool> Execute(string clientId);
    }

    public class RemoveClientAction : IRemoveClientAction
    {
        private readonly IClientRepository _clientRepository;
        private readonly IManagerEventSource _managerEventSource;

        public RemoveClientAction(
            IClientRepository clientRepository,
            IManagerEventSource managerEventSource)
        {
            _clientRepository = clientRepository;
            _managerEventSource = managerEventSource;
        }

        public async Task<bool> Execute(string clientId)
        {
            _managerEventSource.StartToRemoveClient(clientId);
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var client = await _clientRepository.GetClientByIdAsync(clientId).ConfigureAwait(false);
            if (client == null)
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheClientDoesntExist, clientId));
            }

            var result = await _clientRepository.DeleteAsync(client).ConfigureAwait(false);
            if (result)
            {
                _managerEventSource.FinishToRemoveClient(clientId);
            }

            return result;
        }
    }
}
