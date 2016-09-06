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
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Net;

namespace SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions
{
    internal interface IDeleteResourceSetAction
    {
        /// <summary>
        /// throw an <see cref="ArgumentNullException"/> exception when the parameter is null
        /// throw an <see cref="BaseUmaException"/> exception when an error occured while trying to delete the resource set.
        /// false is returned if the resource set doesn't exist
        /// </summary>
        /// <param name="resourceSetId"></param>
        /// <returns></returns>
        bool Execute(string resourceSetId);
    }

    internal class DeleteResourceSetAction : IDeleteResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        private readonly IUmaServerEventSource _umaServerEventSource;

        #region Constructor

        public DeleteResourceSetAction(
            IResourceSetRepository resourceSetRepository,
            IUmaServerEventSource umaServerEventSource)
        {
            _resourceSetRepository = resourceSetRepository;
            _umaServerEventSource = umaServerEventSource;
        }

        #endregion

        #region Public methods

        public bool Execute(string resourceSetId)
        {
            _umaServerEventSource.StartToRemoveResourceSet(resourceSetId);
            if (string.IsNullOrWhiteSpace(resourceSetId))
            {
                throw new ArgumentNullException(nameof(resourceSetId));
            }

            var result = _resourceSetRepository.GetResourceSetById(resourceSetId);
            if (result == null)
            {
                return false;
            }

            if (!_resourceSetRepository.DeleteResource(resourceSetId))
            {
                throw new BaseUmaException(
                    ErrorCodes.InternalError,
                    string.Format(ErrorDescriptions.TheResourceSetCannotBeRemoved, resourceSetId));
            }

            _umaServerEventSource.FinishToRemoveResourceSet(resourceSetId);
            return true;
        }

        #endregion
    }
}
