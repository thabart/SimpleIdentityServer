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
using System.Collections.Generic;
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
        
        [HttpGet("{id}")]
        [Authorize("UmaProtection")]
        public ActionResult GetPolicy(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(id);
            }

            var result = _policyActions.GetPolicy(id);
            if (result == null)
            {
                return GetNotFoundPolicy();
            }

            var content = result.ToResponse();
            return new HttpOkObjectResult(content);
        }

        [HttpGet]
        [Authorize("UmaProtection")]
        public ActionResult GetPolicies()
        {
            var policies = _policyActions.GetPolicies();
            var result = new List<PolicyResponse>();
            foreach(var policy in policies)
            {
                result.Add(policy.ToResponse());
            }

            return new HttpOkObjectResult(result);
        }

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

        [HttpDelete("{id}")]
        [Authorize("UmaProtection")]
        public ActionResult DeletePolicy(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var isPolicyExists = _policyActions.DeletePolicy(id);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            return new HttpStatusCodeResult((int)HttpStatusCode.NoContent);
        }

        #endregion

        #region Private static methods

        private static ActionResult GetNotFoundPolicy()
        {
            var errorResponse = new ErrorResponse
            {
                Error = Constants.ErrorCodes.NotFound,
                ErrorDescription = Constants.ErrorDescriptions.PolicyNotFound
            };

            return new ObjectResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.NotFound
            };
        }

        #endregion
    }
}
