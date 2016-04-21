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

using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Uma.Core.Api.PolicyController;
using SimpleIdentityServer.Uma.Host.DTOs.Requests;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Net;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Policies)]
    public class PoliciesController
    {
        private readonly IPolicyActions _policyActions;

        #region Constructor

        public PoliciesController(IPolicyActions policyActions)
        {
            _policyActions = policyActions;
        }

        #endregion

        #region Public methods

        [HttpPost]
        [Authorize("UmaProtection")]
        public ActionResult PostPolicy([FromBody] PostPolicy postPolicy)
        {
            if (postPolicy == null)
            {
                throw new ArgumentNullException(nameof(postPolicy));
            }

            var policyId = _policyActions.AddPolicy(postPolicy.ToParameter());
            var content = new AddPolicyResponse
            {
                PolicyId = policyId
            };

            return new ObjectResult(content)
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }

        #endregion
    }
}
