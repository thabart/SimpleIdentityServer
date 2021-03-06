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

using Newtonsoft.Json;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Helpers;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.PolicyController.Actions
{
    public interface IUpdatePolicyAction
    {
        Task<bool> Execute(UpdatePolicyParameter updatePolicyParameter);
    }

    internal class UpdatePolicyAction : IUpdatePolicyAction
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public UpdatePolicyAction(IPolicyRepository policyRepository, IRepositoryExceptionHelper repositoryExceptionHelper,
            IResourceSetRepository resourceSetRepository, IUmaServerEventSource umaServerEventSource)
        {
            _policyRepository = policyRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
            _resourceSetRepository = resourceSetRepository;
            _umaServerEventSource = umaServerEventSource;
        }
        
        public async Task<bool> Execute(UpdatePolicyParameter updatePolicyParameter)
        {
            // Check the parameters
            if (updatePolicyParameter == null)
            {
                throw new ArgumentNullException(nameof(updatePolicyParameter));
            }

            if (string.IsNullOrWhiteSpace(updatePolicyParameter.PolicyId))
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "id"));
            }

            if (updatePolicyParameter.Rules == null || !updatePolicyParameter.Rules.Any())
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPolicyParameterNames.Rules));
            }
            
            _umaServerEventSource.StartUpdateAuthorizationPolicy(JsonConvert.SerializeObject(updatePolicyParameter));
            // Check the authorization policy exists.
            var policy = await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, updatePolicyParameter.PolicyId),
                () => _policyRepository.Get(updatePolicyParameter.PolicyId));
            if (policy == null)
            {
                return false;
            }

            policy.Rules = new List<PolicyRule>();
            // Check all the scopes are valid.
            foreach (var resourceSetId in policy.ResourceSetIds)
            {
                var resourceSet = await _resourceSetRepository.Get(resourceSetId);
                if (updatePolicyParameter.Rules.Any(r => r.Scopes != null && !r.Scopes.All(s => resourceSet.Scopes.Contains(s))))
                {
                    throw new BaseUmaException(ErrorCodes.InvalidScope, ErrorDescriptions.OneOrMoreScopesDontBelongToAResourceSet);
                }
            }

            // Update the authorization policy.
            foreach (var ruleParameter in updatePolicyParameter.Rules)
            {
                var claims = new List<Claim>();
                if (ruleParameter.Claims != null)
                {
                    claims = ruleParameter.Claims.Select(c => new Claim
                    {
                        Type = c.Type,
                        Value = c.Value
                    }).ToList();
                }

                policy.Rules.Add(new PolicyRule
                {
                    Id = ruleParameter.Id,
                    ClientIdsAllowed = ruleParameter.ClientIdsAllowed,
                    IsResourceOwnerConsentNeeded = ruleParameter.IsResourceOwnerConsentNeeded,
                    Scopes = ruleParameter.Scopes,
                    Script = ruleParameter.Script,
                    Claims = claims,
                    OpenIdProvider = ruleParameter.OpenIdProvider
                });
            }

            var result = await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeUpdated, updatePolicyParameter.PolicyId),
                () => _policyRepository.Update(policy));
            _umaServerEventSource.FinishUpdateAuhthorizationPolicy(JsonConvert.SerializeObject(updatePolicyParameter));
            return result;
        }
    }
}
