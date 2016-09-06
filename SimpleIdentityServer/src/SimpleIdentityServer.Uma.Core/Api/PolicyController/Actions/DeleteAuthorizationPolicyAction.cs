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
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Logging;
using System;

namespace SimpleIdentityServer.Uma.Core.Api.PolicyController.Actions
{
    public interface IDeleteAuthorizationPolicyAction
    {
        bool Execute(string policyId);
    }

    internal class DeleteAuthorizationPolicyAction : IDeleteAuthorizationPolicyAction
    {
        private readonly IPolicyRepository _policyRepository;

        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;

        private readonly IUmaServerEventSource _umaServerEventSource;

        #region Constructor

        public DeleteAuthorizationPolicyAction(
            IPolicyRepository policyRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper,
            IUmaServerEventSource umaServerEventSource)
        {
            _policyRepository = policyRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
            _umaServerEventSource = umaServerEventSource;
        }

        #endregion

        #region Public methods
        
        public bool Execute(string policyId)
        {
            _umaServerEventSource.StartToRemoveAuthorizationPolicy(policyId);
            if (string.IsNullOrWhiteSpace(policyId))
            {
                throw new ArgumentNullException(nameof(policyId));
            }

            var policy = _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId),
                () => _policyRepository.GetPolicy(policyId));
            if (policy == null)
            {
                return false;
            }

            _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeUpdated, policyId),
                () => _policyRepository.DeletePolicy(policyId));
            _umaServerEventSource.FinishToRemoveAuthorizationPolicy(policyId);
            return true;
        }

        #endregion
    }
}
