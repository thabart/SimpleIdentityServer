﻿#region copyright
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.PolicyController.Actions
{
    public interface IGetAuthorizationPoliciesAction
    {
        Task<ICollection<string>> Execute();
    }

    internal class GetAuthorizationPoliciesAction : IGetAuthorizationPoliciesAction
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;

        public GetAuthorizationPoliciesAction(
            IPolicyRepository policyRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper)
        {
            _policyRepository = policyRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
        }
        
        public async Task<ICollection<string>> Execute()
        {
            var policies = await _repositoryExceptionHelper.HandleException(
                ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved,
                () => _policyRepository.GetAll()).ConfigureAwait(false);
            if (policies == null
                || !policies.Any())
            {
                return new List<string>();
            }

            return policies.Select(p => p.Id).ToList();
        }
    }
}
