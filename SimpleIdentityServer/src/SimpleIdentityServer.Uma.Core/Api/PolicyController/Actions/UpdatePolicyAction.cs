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

using System;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Errors;

namespace SimpleIdentityServer.Uma.Core.Api.PolicyController.Actions
{
    public interface IUpdatePolicyAction
    {
        bool Execute(UpdatePolicyParameter updatePolicyParameter);
    }

    internal class UpdatePolicyAction : IUpdatePolicyAction
    {
        private readonly IPolicyRepository _policyRepository;

        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;

        #region Constructor

        public UpdatePolicyAction(IPolicyRepository policyRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper)
        {
            _policyRepository = policyRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
        }

        #endregion

        #region Public methods

        public bool Execute(UpdatePolicyParameter updatePolicyParameter)
        {
            if (updatePolicyParameter == null)
            {
                throw new ArgumentNullException(nameof(updatePolicyParameter));
            }

            var policy = _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, updatePolicyParameter.PolicyId),
                () => _policyRepository.GetPolicy(updatePolicyParameter.PolicyId));
            if (policy == null)
            {
                return false;
            }

            policy.ClientIdsAllowed = updatePolicyParameter.ClientIdsAllowed;
            policy.Scopes = updatePolicyParameter.Scopes;
            policy.IsResourceOwnerConsentNeeded = updatePolicyParameter.IsResourceOwnerConsentNeeded;
            policy.Script = updatePolicyParameter.Script;
            return _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeUpdated, updatePolicyParameter.PolicyId),
                () => _policyRepository.UpdatePolicy(policy));
        }

        #endregion
    }
}
