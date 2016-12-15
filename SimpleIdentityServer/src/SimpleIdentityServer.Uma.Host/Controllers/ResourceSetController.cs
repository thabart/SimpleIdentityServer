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
using SimpleIdentityServer.Uma.Common.DTOs;
using SimpleIdentityServer.Uma.Core.Api.ResourceSetController;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;
using WebApiContrib.Core.Concurrency;
using static SimpleIdentityServer.Uma.Host.Constants;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(RouteValues.ResourceSet)]
    public class ResourceSetController : Controller
    {
        private readonly IResourceSetActions _resourceSetActions;
        private readonly IRepresentationManager _representationManager;

        public ResourceSetController(
            IResourceSetActions resourceSetActions,
            IRepresentationManager representationManager)
        {
            _resourceSetActions = resourceSetActions;
            _representationManager = representationManager;
        }

        [HttpGet]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> GetResourceSets()
        {
            if (!await _representationManager.CheckRepresentationExistsAsync(this, CachingStoreNames.GetResourcesStoreName))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var resourceSetIds = await _resourceSetActions.GetAllResourceSet();
            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetResourcesStoreName);
            return new OkObjectResult(resourceSetIds);
        }

        [HttpGet("{id}")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> GetResourceSet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!await _representationManager.CheckRepresentationExistsAsync(this, CachingStoreNames.GetResourceStoreName + id))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result = await _resourceSetActions.GetResourceSet(id);
            if (result == null)
            {
                return GetNotFoundResourceSet();
            }

            var content = result.ToResponse();
            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetResourceStoreName + id);
            return new OkObjectResult(content);
        }

        [HttpPost]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> AddResourceSet([FromBody] PostResourceSet postResourceSet)
        {
            if (postResourceSet == null)
            {
                throw new ArgumentNullException(nameof(postResourceSet));
            }

            var parameter = postResourceSet.ToParameter();
            var result = await _resourceSetActions.AddResourceSet(parameter);
            var response = new AddResourceSetResponse
            {
                Id = result
            };
            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetResourcesStoreName, false);
            return new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }

        [HttpPut]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> UpdateResourceSet([FromBody] PutResourceSet putResourceSet)
        {
            if (putResourceSet == null)
            {
                throw new ArgumentNullException(nameof(putResourceSet));
            }

            var parameter = putResourceSet.ToParameter();
            var resourceSetExists = await _resourceSetActions.UpdateResourceSet(parameter);
            if (!resourceSetExists)
            {
                return GetNotFoundResourceSet();
            }

            var response = new UpdateResourceSetResponse
            {
                Id = putResourceSet.Id
            };

            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetResourceStoreName + putResourceSet.Id, false);
            return new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        [HttpDelete("{id}")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> DeleteResourceSet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var policyIds = await _resourceSetActions.GetPolicies(id);
            var resourceSetExists = await _resourceSetActions.RemoveResourceSet(id);
            if (!resourceSetExists)
            {
                return GetNotFoundResourceSet();
            }

            // Update all the representations include the authorization policies
            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetResourceStoreName + id, false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetResourcesStoreName, false);
            foreach (var policyId in policyIds)
            {
                await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + policyId, false);
            }

            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        private static ActionResult GetNotFoundResourceSet()
        {
            var errorResponse = new ErrorResponse
            {
                Error = Constants.ErrorCodes.NotFound,
                ErrorDescription = Constants.ErrorDescriptions.ResourceSetNotFound
            };

            return new ObjectResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.NotFound
            };
        }
    }
}
