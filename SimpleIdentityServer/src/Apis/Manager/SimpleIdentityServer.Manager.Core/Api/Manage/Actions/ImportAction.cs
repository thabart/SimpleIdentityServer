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

using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Manage.Actions
{
    public interface IImportAction
    {
        Task<bool> Execute(ImportParameter importParameter);
    }

    internal class ImportAction : IImportAction
    {
        private readonly IClientRepository _clientRepository;
        private readonly IManagerEventSource _managerEventSource;

        public ImportAction(
            IClientRepository clientRepository,
            IManagerEventSource managerEventSource)
        {
            if(clientRepository == null)
            {
                throw new ArgumentNullException(nameof(clientRepository));
            }

            if (managerEventSource == null)
            {
                throw new ArgumentNullException(nameof(managerEventSource));
            }

            _clientRepository = clientRepository;
            _managerEventSource = managerEventSource;
        }
        
        public async Task<bool> Execute(ImportParameter importParameter)
        {
            if (importParameter == null)
            {
                throw new ArgumentNullException(nameof(importParameter));
            }

            if (importParameter.Clients == null)
            {
                throw new ArgumentNullException(nameof(importParameter.Clients));
            }

            _managerEventSource.StartToImport();

            // 1. Remove all the clients
            if (!await _clientRepository.RemoveAllAsync())
            {
                return false;
            }

            _managerEventSource.RemoveAllClients();

            // 2. Import the clients
            foreach (var client in importParameter.Clients)
            {
                try
                {
                    await _clientRepository.InsertAsync(client);
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                }
            }

            _managerEventSource.FinishToImport();
            return true;
        }
    }
}
