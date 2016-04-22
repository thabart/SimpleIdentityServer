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

namespace SimpleIdentityServer.Uma.Core.Api.PolicyController
{
    public interface IPolicyActions
    {
        string AddPolicy(AddPolicyParameter addPolicyParameter);

        Policy GetPolicy(string policyId);

        bool DeletePolicy(string policyId);

        List<string> GetPolicies();
    }

    internal class PolicyActions : IPolicyActions
    {
        private readonly IAddAuthorizationPolicyAction _addAuthorizationPolicyAction;

        private readonly IGetAuthorizationPolicyAction _getAuthorizationPolicyAction;

        private readonly IDeleteAuthorizationPolicyAction _deleteAuthorizationPolicyAction;

        private readonly IGetAuthorizationPoliciesAction _getAuthorizationPoliciesAction;

        #region  Constructor

        public PolicyActions(
            IAddAuthorizationPolicyAction addAuthorizationPolicyAction,
            IGetAuthorizationPolicyAction getAuthorizationPolicyAction,
            IDeleteAuthorizationPolicyAction deleteAuthorizationPolicyAction,
            IGetAuthorizationPoliciesAction getAuthorizationPoliciesAction)
        {
            _addAuthorizationPolicyAction = addAuthorizationPolicyAction;
            _getAuthorizationPolicyAction = getAuthorizationPolicyAction;
            _deleteAuthorizationPolicyAction = deleteAuthorizationPolicyAction;
            _getAuthorizationPoliciesAction = getAuthorizationPoliciesAction;
        }

        #endregion

        #region Public methods

        public string AddPolicy(AddPolicyParameter addPolicyParameter)
        {
            return _addAuthorizationPolicyAction.Execute(addPolicyParameter);
        }

        public Policy GetPolicy(string policyId)
        {
            return _getAuthorizationPolicyAction.Execute(policyId);
        }

        public bool DeletePolicy(string policyId)
        {
            return _deleteAuthorizationPolicyAction.Execute(policyId);
        }

        public List<string> GetPolicies()
        {
            return _getAuthorizationPoliciesAction.Execute();
        }

        #endregion
    }
}
