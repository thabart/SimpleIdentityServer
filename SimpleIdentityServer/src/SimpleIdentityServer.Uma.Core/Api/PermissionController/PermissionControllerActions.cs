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

using SimpleIdentityServer.Uma.Core.Api.PermissionController.Actions;
using SimpleIdentityServer.Uma.Core.Parameters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.PermissionController
{
    public interface IPermissionControllerActions
    {
        Task<string> Add(AddPermissionParameter addPermissionParameter, string clientId);
        Task<IEnumerable<string>> Add(IEnumerable<AddPermissionParameter> addPermissionParameters, string clientId);
    }

    internal class PermissionControllerActions : IPermissionControllerActions
    {
        private readonly IAddPermissionAction _addPermissionAction;

        public PermissionControllerActions(IAddPermissionAction addPermissionAction)
        {
            _addPermissionAction = addPermissionAction;
        }

        public Task<string> Add(AddPermissionParameter addPermissionParameter, string clientId)
        {
            return _addPermissionAction.Execute(clientId, addPermissionParameter);
        }

        public Task<IEnumerable<string>> Add(IEnumerable<AddPermissionParameter> addPermissionParameters, string clientId)
        {
            return _addPermissionAction.Execute(clientId, addPermissionParameters);
        }
    }
}
