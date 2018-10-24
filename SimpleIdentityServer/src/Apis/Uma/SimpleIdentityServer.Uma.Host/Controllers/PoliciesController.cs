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
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Uma.Common.DTOs;
using SimpleIdentityServer.Uma.Core.Api.PolicyController;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Host.Extensions;
using SimpleIdServer.Concurrency;
using System.Net;
using System.Threading.Tasks;
using static SimpleIdentityServer.Uma.Host.Constants;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Policies)]
    public class PoliciesController : Controller
    {
        private readonly IPolicyActions _policyActions;
        private readonly IRepresentationManager _representationManager;

        public PoliciesController(
            IPolicyActions policyActions,
            IRepresentationManager representationManager)
        {
            _policyActions = policyActions;
            _representationManager = representationManager;
        }

        [HttpPost(".search")]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> SearchPolicies([FromBody] SearchAuthPolicies searchAuthPolicies)
        {
            if (searchAuthPolicies == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var parameter = searchAuthPolicies.ToParameter();
            var result = await _policyActions.Search(parameter);
            return new OkObjectResult(result.ToResponse());
        }

        [HttpGet("{id}")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> GetPolicy(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the identifier must be specified", HttpStatusCode.BadRequest);
            }

            if (!await _representationManager.CheckRepresentationExistsAsync(this, CachingStoreNames.GetPolicyStoreName + id))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result = await _policyActions.GetPolicy(id);
            if (result == null)
            {
                return GetNotFoundPolicy();
            }

            var content = result.ToResponse();
            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + id);
            return new OkObjectResult(content);
        }

        [HttpGet]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> GetPolicies()
        {
            var policies = await _policyActions.GetPolicies().ConfigureAwait(false);
            return new OkObjectResult(policies);
        }

        // Partial update
        [HttpPut]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> PutPolicy([FromBody] PutPolicy putPolicy)
        {
            if (putPolicy == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var isPolicyExists = await _policyActions.UpdatePolicy(putPolicy.ToParameter());
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + putPolicy.PolicyId, false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }
        
        [HttpPost("{id}/resources")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> PostAddResourceSet(string id, [FromBody] PostAddResourceSet postAddResourceSet)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the identifier must be specified", HttpStatusCode.BadRequest);
            }

            if (postAddResourceSet == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var isPolicyExists = await _policyActions.AddResourceSet(new AddResourceSetParameter
            {
                PolicyId = id,
                ResourceSets = postAddResourceSet.ResourceSets
            });
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + id, false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        [HttpDelete("{id}/resources/{resourceId}")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> DeleteResourceSet(string id, string resourceId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the identifier must be specified", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the resource_id must be specified", HttpStatusCode.BadRequest);
            }

            var isPolicyExists = await _policyActions.DeleteResourceSet(id, resourceId);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + id, false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> PostPolicy([FromBody] PostPolicy postPolicy)
        {
            if (postPolicy == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var policyId = await _policyActions.AddPolicy(postPolicy.ToParameter());
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
        public async Task<ActionResult> DeletePolicy(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the identifier must be specified", HttpStatusCode.BadRequest);
            }

            var isPolicyExists = await _policyActions.DeletePolicy(id);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + id, false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        private static ActionResult GetNotFoundPolicy()
        {
            var errorResponse = new ErrorResponse
            {
                Error = "not_found",
                ErrorDescription = "policy cannot be found"
            };

            return new ObjectResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.NotFound
            };
        }

        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorResponse
            {
                Error = code,
                ErrorDescription = message
            };
            return new JsonResult(error)
            {
                StatusCode = (int)statusCode
            };
        }
    }
}
