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
using System;
using SimpleIdentityServer.Manager.Core.Api.ResourceOwners.Actions;

namespace SimpleIdentityServer.Manager.Core.Api.ResourceOwners
{
    public interface IResourceOwnerActions
    {
        bool UpdateResourceOwner(UpdateResourceOwnerParameter updateResourceOwnerParameter);

        ResourceOwner GetResourceOwner(string subject);

        List<ResourceOwner> GetResourceOwners();

        bool Delete(string subject);
    }

    internal class ResourceOwnerActions : IResourceOwnerActions
    {
        #region Fields

        private readonly IGetResourceOwnerAction _getResourceOwnerAction;

        private readonly IGetResourceOwnersAction _getResourceOwnersAction;

        private readonly IUpdateResourceOwnerAction _updateResourceOwnerAction;

        private readonly IDeleteResourceOwnerAction _deleteResourceOwnerAction;

        #endregion

        #region Constructor

        public ResourceOwnerActions(
            IGetResourceOwnerAction getResourceOwnerAction,
            IGetResourceOwnersAction getResourceOwnersAction,
            IUpdateResourceOwnerAction updateResourceOwnerAction,
            IDeleteResourceOwnerAction deleteResourceOwnerAction)
        {
            _getResourceOwnerAction = getResourceOwnerAction;
            _getResourceOwnersAction = getResourceOwnersAction;
            _updateResourceOwnerAction = updateResourceOwnerAction;
            _deleteResourceOwnerAction = deleteResourceOwnerAction;
        }

        #endregion

        #region Public methods

        public ResourceOwner GetResourceOwner(string subject)
        {
            return _getResourceOwnerAction.Execute(subject);
        }

        public List<ResourceOwner> GetResourceOwners()
        {
            return _getResourceOwnersAction.Execute();
        }

        public bool UpdateResourceOwner(UpdateResourceOwnerParameter updateResourceOwnerParameter)
        {
            return _updateResourceOwnerAction.Execute(updateResourceOwnerParameter);
        }

        public bool Delete(string subject)
        {
            return _deleteResourceOwnerAction.Execute(subject);
        }

        #endregion
    }
}
