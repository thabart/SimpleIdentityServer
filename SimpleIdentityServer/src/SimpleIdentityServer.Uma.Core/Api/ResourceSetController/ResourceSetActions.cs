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

using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions;
using SimpleIdentityServer.Uma.Core.Models;
using System.Net;

namespace SimpleIdentityServer.Uma.Core.Api.ResourceSetController
{
    public interface IResourceSetActions
    {
        string AddResourceSet(AddResouceSetParameter addResouceSetParameter);

        ResourceSet GetResourceSet(string id);

        string UpdateResourceSet(UpdateResourceSetParameter updateResourceSetParameter);

        HttpStatusCode RemoveResourceSet(string resourceSetId);
    }

    internal class ResourceSetActions : IResourceSetActions
    {
        private readonly IAddResourceSetAction _addResourceSetAction;

        private readonly IGetResourceSetAction _getResourceSetAction;

        private readonly IUpdateResourceSetAction _updateResourceSetAction;

        private readonly IDeleteResourceSetAction _deleteResourceSetAction;

        #region Constructor

        public ResourceSetActions(
            IAddResourceSetAction addResourceSetAction,
            IGetResourceSetAction getResourceSetAction,
            IUpdateResourceSetAction updateResourceSetAction,
            IDeleteResourceSetAction deleteResourceSetAction)
        {
            _addResourceSetAction = addResourceSetAction;
            _getResourceSetAction = getResourceSetAction;
            _updateResourceSetAction = updateResourceSetAction;
            _deleteResourceSetAction = deleteResourceSetAction;
        }
        
        #endregion
        
        #region Public methods
        
        public string AddResourceSet(AddResouceSetParameter addResouceSetParameter)
        {
            return _addResourceSetAction.Execute(addResouceSetParameter);
        }

        public ResourceSet GetResourceSet(string id)
        {
            return _getResourceSetAction.Execute(id);
        }

        public string UpdateResourceSet(UpdateResourceSetParameter updateResourceSetParameter)
        {
            return _updateResourceSetAction.Execute(updateResourceSetParameter);
        }

        public HttpStatusCode RemoveResourceSet(string resourceSetId)
        {
            return _deleteResourceSetAction.Execute(resourceSetId);
        }

        #endregion
    }
}
