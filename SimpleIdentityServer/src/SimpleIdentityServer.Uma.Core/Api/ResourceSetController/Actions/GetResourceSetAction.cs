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

using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;

namespace SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions
{
    internal interface IGetResourceSetAction
    {
        ResourceSet Execute(string id);
    }

    internal class GetResourceSetAction : IGetResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        #region Constructor

        public GetResourceSetAction(IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
        }

        #endregion

        #region Public methods

        public ResourceSet Execute(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var resourceSet = _resourceSetRepository.GetResourceSetById(id);
            if (resourceSet == null)
            {
                throw new BaseUmaException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheResourceSetDoesntExist, id));
            }

            return resourceSet;
        }

        #endregion
    }
}
