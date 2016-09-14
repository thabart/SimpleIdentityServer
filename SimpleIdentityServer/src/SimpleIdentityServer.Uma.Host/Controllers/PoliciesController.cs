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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Uma.Core.Api.PolicyController;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Host.DTOs.Requests;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;
using WebApiContrib.Core.Concurrency;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Policies)]
    public class PoliciesController : Controller
    {
        private const string GetPoliciesStoreName = "GetPolicies";

        private const string GetPolicyStoreName = "GetPolicy_";

        private readonly IPolicyActions _policyActions;

        private readonly IRepresentationManager _representationManager;

        #region Constructor

        public PoliciesController(
            IPolicyActions policyActions,
            IRepresentationManager representationManager)
        {
            _policyActions = policyActions;
            _representationManager = representationManager;
        }

        #endregion

        #region Public methods
        
        [HttpGet("{id}")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> GetPolicy(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(id);
            }

            if (!await _representationManager.CheckRepresentationExistsAsync(this, GetPolicyStoreName + id))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result = _policyActions.GetPolicy(id);
            if (result == null)
            {
                return GetNotFoundPolicy();
            }

            var content = result.ToResponse();
            await _representationManager.AddOrUpdateRepresentationAsync(this, GetPolicyStoreName + id);
            return new OkObjectResult(content);
        }

        [HttpGet]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> GetPolicies()
        {
            if (!await _representationManager.CheckRepresentationExistsAsync(this, GetPoliciesStoreName))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var policies = _policyActions.GetPolicies();
            await _representationManager.AddOrUpdateRepresentationAsync(this, GetPoliciesStoreName);
            return new OkObjectResult(policies);
        }

        // Partial update
        [HttpPut]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> PutPolicy([FromBody] PutPolicy putPolicy)
        {
            if (putPolicy == null)
            {
                throw new ArgumentNullException(nameof(putPolicy));
            }

            var isPolicyExists = _policyActions.UpdatePolicy(putPolicy.ToParameter());
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, GetPolicyStoreName + putPolicy.PolicyId, false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }
        
        [HttpPost("{id}/resources")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> PostAddResourceSet(string id, [FromBody] PostAddResourceSet postAddResourceSet)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (postAddResourceSet == null)
            {
                throw new ArgumentNullException(nameof(postAddResourceSet));
            }

            var isPolicyExists = _policyActions.AddResourceSet(new AddResourceSetParameter
            {
                PolicyId = id,
                ResourceSets = postAddResourceSet.ResourceSets
            });
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, GetPolicyStoreName + id, false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        [HttpDelete("{id}/resources/{resourceId}")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> DeleteResourceSet(string id, string resourceId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            var isPolicyExists = _policyActions.DeleteResourceSet(id, resourceId);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, GetPolicyStoreName + id, false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> PostPolicy([FromBody] PostPolicy postPolicy)
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

            await _representationManager.AddOrUpdateRepresentationAsync(this, GetPoliciesStoreName, false);
            return new ObjectResult(content)
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }

        [HttpDelete("{id}")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> DeletePolicy(string id)
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

            await _representationManager.AddOrUpdateRepresentationAsync(this, GetPolicyStoreName + id, false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, GetPoliciesStoreName, false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
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
