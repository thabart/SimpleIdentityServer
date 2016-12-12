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

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Manager.Core.Parameters;
using System.Collections.Generic;
using SimpleIdentityServer.Manager.Core.Api.ResourceOwners.Actions;
using SimpleIdentityServer.Core.WebSite.Account.Actions;
using SimpleIdentityServer.Core.Parameters;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.ResourceOwners
{
    public interface IResourceOwnerActions
    {
        Task<bool> UpdateResourceOwner(ResourceOwner resourceOwner);
        Task<ResourceOwner> GetResourceOwner(string subject);
        Task<ICollection<ResourceOwner>> GetResourceOwners();
        Task<bool> Delete(string subject);
        Task Add(AddUserParameter parameter);
    }

    internal class ResourceOwnerActions : IResourceOwnerActions
    {
        private readonly IGetResourceOwnerAction _getResourceOwnerAction;
        private readonly IGetResourceOwnersAction _getResourceOwnersAction;
        private readonly IUpdateResourceOwnerAction _updateResourceOwnerAction;
        private readonly IDeleteResourceOwnerAction _deleteResourceOwnerAction;
        private readonly IAddResourceOwnerAction _addResourceOwnerAction;

        public ResourceOwnerActions(
            IGetResourceOwnerAction getResourceOwnerAction,
            IGetResourceOwnersAction getResourceOwnersAction,
            IUpdateResourceOwnerAction updateResourceOwnerAction,
            IDeleteResourceOwnerAction deleteResourceOwnerAction,
            IAddResourceOwnerAction addResourceOwnerAction)
        {
            _getResourceOwnerAction = getResourceOwnerAction;
            _getResourceOwnersAction = getResourceOwnersAction;
            _updateResourceOwnerAction = updateResourceOwnerAction;
            _deleteResourceOwnerAction = deleteResourceOwnerAction;
            _addResourceOwnerAction = addResourceOwnerAction;
        }
        
        public Task<ResourceOwner> GetResourceOwner(string subject)
        {
            return _getResourceOwnerAction.Execute(subject);
        }

        public Task<ICollection<ResourceOwner>> GetResourceOwners()
        {
            return _getResourceOwnersAction.Execute();
        }

        public Task<bool> UpdateResourceOwner(ResourceOwner resourceOwner)
        {
            return _updateResourceOwnerAction.Execute(resourceOwner);
        }

        public Task<bool> Delete(string subject)
        {
            return _deleteResourceOwnerAction.Execute(subject);
        }

        public Task Add(AddUserParameter parameter)
        {
            return _addResourceOwnerAction.Execute(parameter);
        }
    }
}
