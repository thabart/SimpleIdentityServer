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
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions
{
    internal interface IDeleteResourceSetAction
    {
        Task<bool> Execute(string resourceSetId);
    }

    internal class DeleteResourceSetAction : IDeleteResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public DeleteResourceSetAction(
            IResourceSetRepository resourceSetRepository,
            IUmaServerEventSource umaServerEventSource)
        {
            _resourceSetRepository = resourceSetRepository;
            _umaServerEventSource = umaServerEventSource;
        }

        public async Task<bool> Execute(string resourceSetId)
        {
            _umaServerEventSource.StartToRemoveResourceSet(resourceSetId);
            if (string.IsNullOrWhiteSpace(resourceSetId))
            {
                throw new ArgumentNullException(nameof(resourceSetId));
            }

            var result = await _resourceSetRepository.Get(resourceSetId).ConfigureAwait(false);
            if (result == null)
            {
                return false;
            }

            if (!await _resourceSetRepository.Delete(resourceSetId).ConfigureAwait(false))
            {
                throw new BaseUmaException(
                    ErrorCodes.InternalError,
                    string.Format(ErrorDescriptions.TheResourceSetCannotBeRemoved, resourceSetId));
            }

            _umaServerEventSource.FinishToRemoveResourceSet(resourceSetId);
            return true;
        }
    }
}
