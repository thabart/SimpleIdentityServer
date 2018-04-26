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

using SimpleIdentityServer.Uma.Core.Api.PolicyController.Actions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.PolicyController
{
    public interface IPolicyActions
    {
        Task<string> AddPolicy(AddPolicyParameter addPolicyParameter);
        Task<Policy> GetPolicy(string policyId);
        Task<bool> DeletePolicy(string policyId);
        Task<bool> UpdatePolicy(UpdatePolicyParameter updatePolicyParameter);
        Task<ICollection<string>> GetPolicies();
        Task<bool> AddResourceSet(AddResourceSetParameter addResourceSetParameter);
        Task<bool> DeleteResourceSet(string id, string resourceId);
        Task<SearchAuthPoliciesResult> Search(SearchAuthPoliciesParameter parameter);
    }

    internal class PolicyActions : IPolicyActions
    {
        private readonly IAddAuthorizationPolicyAction _addAuthorizationPolicyAction;
        private readonly IGetAuthorizationPolicyAction _getAuthorizationPolicyAction;
        private readonly IDeleteAuthorizationPolicyAction _deleteAuthorizationPolicyAction;
        private readonly IGetAuthorizationPoliciesAction _getAuthorizationPoliciesAction;
        private readonly IUpdatePolicyAction _updatePolicyAction;
        private readonly IAddResourceSetToPolicyAction _addResourceSetAction;
        private readonly IDeleteResourcePolicyAction _deleteResourcePolicyAction;
        private readonly ISearchAuthPoliciesAction _searchAuthPoliciesAction;

        public PolicyActions(
            IAddAuthorizationPolicyAction addAuthorizationPolicyAction,
            IGetAuthorizationPolicyAction getAuthorizationPolicyAction,
            IDeleteAuthorizationPolicyAction deleteAuthorizationPolicyAction,
            IGetAuthorizationPoliciesAction getAuthorizationPoliciesAction,
            IUpdatePolicyAction updatePolicyAction,
            IAddResourceSetToPolicyAction addResourceSetAction,
            IDeleteResourcePolicyAction deleteResourcePolicyAction,
            ISearchAuthPoliciesAction searchAuthPoliciesAction)
        {
            _addAuthorizationPolicyAction = addAuthorizationPolicyAction;
            _getAuthorizationPolicyAction = getAuthorizationPolicyAction;
            _deleteAuthorizationPolicyAction = deleteAuthorizationPolicyAction;
            _getAuthorizationPoliciesAction = getAuthorizationPoliciesAction;
            _updatePolicyAction = updatePolicyAction;
            _addResourceSetAction = addResourceSetAction;
            _deleteResourcePolicyAction = deleteResourcePolicyAction;
            _searchAuthPoliciesAction = searchAuthPoliciesAction;
        }

        public Task<SearchAuthPoliciesResult> Search(SearchAuthPoliciesParameter parameter)
        {
            return _searchAuthPoliciesAction.Execute(parameter);
        }

        public Task<string> AddPolicy(AddPolicyParameter addPolicyParameter)
        {
            return _addAuthorizationPolicyAction.Execute(addPolicyParameter);
        }

        public Task<Policy> GetPolicy(string policyId)
        {
            return _getAuthorizationPolicyAction.Execute(policyId);
        }

        public Task<bool> DeletePolicy(string policyId)
        {
            return _deleteAuthorizationPolicyAction.Execute(policyId);
        }

        public Task<ICollection<string>> GetPolicies()
        {
            return _getAuthorizationPoliciesAction.Execute();
        }

        public Task<bool> UpdatePolicy(UpdatePolicyParameter updatePolicyParameter)
        {
            return _updatePolicyAction.Execute(updatePolicyParameter);
        }

        public Task<bool> AddResourceSet(AddResourceSetParameter addResourceSetParameter)
        {
            return _addResourceSetAction.Execute(addResourceSetParameter);
        }

        public Task<bool> DeleteResourceSet(string id, string resourceId)
        {
            return _deleteResourcePolicyAction.Execute(id, resourceId);
        }
    }
}
