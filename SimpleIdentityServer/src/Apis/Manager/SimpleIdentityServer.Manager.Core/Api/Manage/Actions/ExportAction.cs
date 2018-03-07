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

using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Manager.Core.Api.Clients.Actions;
using SimpleIdentityServer.Manager.Core.Results;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Manage.Actions
{
    public interface IExportAction
    {
        Task<ExportResult> Execute();
    }

    internal class ExportAction : IExportAction
    {
        private readonly IGetClientsAction _getClientsAction;        
        private readonly IManagerEventSource _managerEventSource;

        public ExportAction(IGetClientsAction  getClientsAction, IManagerEventSource managerEventSource)
        {
            if (getClientsAction == null)
            {
                throw new ArgumentNullException(nameof(getClientsAction));
            }

            if (managerEventSource == null)
            {
                throw new ArgumentNullException(nameof(managerEventSource));
            }

            _getClientsAction = getClientsAction;
            _managerEventSource = managerEventSource;
        }
        
        public async Task<ExportResult> Execute()
        {
            _managerEventSource.StartToExport();
            var result = new ExportResult
            {
                Clients = await _getClientsAction.Execute()
            };
            _managerEventSource.FinishToExport();
            return result;
        }
    }
}
