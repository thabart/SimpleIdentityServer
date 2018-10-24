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
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.PolicyController.Actions
{
    public interface IDeleteResourcePolicyAction
    {
        Task<bool> Execute(string id, string resourceId);
    }

    internal class DeleteResourcePolicyAction : IDeleteResourcePolicyAction
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public DeleteResourcePolicyAction(
            IPolicyRepository policyRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper,
            IResourceSetRepository resourceSetRepository,
            IUmaServerEventSource umaServerEventSource)
        {
            _policyRepository = policyRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
            _resourceSetRepository = resourceSetRepository;
            _umaServerEventSource = umaServerEventSource;
        }
        
        public async Task<bool> Execute(string id, string resourceId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            _umaServerEventSource.StartRemoveResourceFromAuthorizationPolicy(id, resourceId);
            var policy = await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, id),
                () => _policyRepository.Get(id));
            if (policy == null)
            {
                return false;
            }

            var resourceSet = await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceId),
                () => _resourceSetRepository.Get(resourceId));
            if (resourceSet == null)
            {
                throw new BaseUmaException(ErrorCodes.InvalidResourceSetId,
                    string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceId));
            }

            if (policy.ResourceSetIds == null ||
                !policy.ResourceSetIds.Contains(resourceId))
            {
                throw new BaseUmaException(ErrorCodes.InvalidResourceSetId,
                    ErrorDescriptions.ThePolicyDoesntContainResource);
            }

            policy.ResourceSetIds.Remove(resourceId);
            var result = await _policyRepository.Update(policy);
            _umaServerEventSource.FinishRemoveResourceFromAuthorizationPolicy(id, resourceId);
            return result;
        }
    }
}
