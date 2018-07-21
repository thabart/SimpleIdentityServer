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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Uma.Common.DTOs;
using SimpleIdentityServer.Uma.Core.Api.PolicyController;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;
using WebApiContrib.Core.Concurrency;
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
        
        [HttpGet("{id}")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> GetPolicy(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(id);
            }

            if (!await _representationManager.CheckRepresentationExistsAsync(this, CachingStoreNames.GetPolicyStoreName + id).ConfigureAwait(false))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result = await _policyActions.GetPolicy(id).ConfigureAwait(false);
            if (result == null)
            {
                return GetNotFoundPolicy();
            }

            var content = result.ToResponse();
            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + id).ConfigureAwait(false);
            return new OkObjectResult(content);
        }

        [HttpGet]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> GetPolicies()
        {
            if (!await _representationManager.CheckRepresentationExistsAsync(this, CachingStoreNames.GetPoliciesStoreName).ConfigureAwait(false))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var policies = await _policyActions.GetPolicies().ConfigureAwait(false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPoliciesStoreName).ConfigureAwait(false);
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

            var isPolicyExists = await _policyActions.UpdatePolicy(putPolicy.ToParameter()).ConfigureAwait(false);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + putPolicy.PolicyId, false).ConfigureAwait(false);
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

            var isPolicyExists = await _policyActions.AddResourceSet(new AddResourceSetParameter
            {
                PolicyId = id,
                ResourceSets = postAddResourceSet.ResourceSets
            }).ConfigureAwait(false);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + id, false).ConfigureAwait(false);
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

            var isPolicyExists = await _policyActions.DeleteResourceSet(id, resourceId).ConfigureAwait(false);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + id, false).ConfigureAwait(false);
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

            var policyId = await _policyActions.AddPolicy(postPolicy.ToParameter()).ConfigureAwait(false);
            var content = new AddPolicyResponse
            {
                PolicyId = policyId
            };

            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPoliciesStoreName, false).ConfigureAwait(false);
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

            var isPolicyExists = await _policyActions.DeletePolicy(id).ConfigureAwait(false);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + id, false).ConfigureAwait(false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPoliciesStoreName, false).ConfigureAwait(false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

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
    }
}
