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

using SimpleIdentityServer.Manager.Core.Api.Manage.Actions;
using SimpleIdentityServer.Manager.Core.Parameters;
using SimpleIdentityServer.Manager.Core.Results;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Manage
{
    public interface IManageActions
    {
        Task<ExportResult> Export();
        Task<bool> Import(ImportParameter importParameter);
    }

    internal class ManageActions : IManageActions
    {
        private readonly IExportAction _exportAction;
        private readonly IImportAction _importAction;

        public ManageActions(
            IExportAction exportAction,
            IImportAction importAction)
        {
            if (exportAction == null)
            {
                throw new ArgumentNullException(nameof(exportAction));
            }

            if (importAction == null)
            {
                throw new ArgumentNullException(nameof(importAction));
            }

            _exportAction = exportAction;
            _importAction = importAction;
        }

        public Task<ExportResult> Export()
        {
            return _exportAction.Execute();
        }

        public Task<bool> Import(ImportParameter importParameter)
        {
            return _importAction.Execute(importParameter);
        }
    }
}
