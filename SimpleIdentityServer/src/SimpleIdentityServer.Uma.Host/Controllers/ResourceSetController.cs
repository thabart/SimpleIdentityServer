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
using SimpleIdentityServer.Uma.Core.Api.ResourceSetController;
using SimpleIdentityServer.Uma.Host.DTOs.Requests;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;
using WebApiContrib.Core.Concurrency;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.ResourceSet)]
    public class ResourceSetController : Controller
    {
        private const string GetResourcesStoreName = "GetResources";

        private const string GetResourceStoreName = "GetResource_";

        private readonly IResourceSetActions _resourceSetActions;

        private readonly IRepresentationManager _representationManager;

        #region Constructor

        public ResourceSetController(
            IResourceSetActions resourceSetActions,
            IRepresentationManager representationManager)
        {
            _resourceSetActions = resourceSetActions;
            _representationManager = representationManager;
        }

        #endregion

        #region Public methods

        [HttpGet]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> GetResourceSets()
        {
            if (!await _representationManager.CheckRepresentationExistsAsync(this, GetResourcesStoreName))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var resourceSetIds = _resourceSetActions.GetAllResourceSet();
            await _representationManager.AddOrUpdateRepresentationAsync(this, GetResourcesStoreName);
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

            if (!await _representationManager.CheckRepresentationExistsAsync(this, GetResourceStoreName + id))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result = _resourceSetActions.GetResourceSet(id);
            if (result == null)
            {
                return GetNotFoundResourceSet();
            }

            var content = result.ToResponse();
            await _representationManager.AddOrUpdateRepresentationAsync(this, GetResourceStoreName + id);
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
            var result = _resourceSetActions.AddResourceSet(parameter);
            var response = new AddResourceSetResponse
            {
                Id = result
            };
            await _representationManager.AddOrUpdateRepresentationAsync(this, GetResourcesStoreName, false);
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
            var resourceSetExists = _resourceSetActions.UpdateResourceSet(parameter);
            if (!resourceSetExists)
            {
                return GetNotFoundResourceSet();
            }

            var response = new UpdateSetResponse
            {
                Id = putResourceSet.Id
            };

            await _representationManager.AddOrUpdateRepresentationAsync(this, GetResourceStoreName + putResourceSet.Id, false);
            return new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        [HttpDelete("{id}")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult> DeleleteResourceSet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var resourceSetExists = _resourceSetActions.RemoveResourceSet(id);
            if (!resourceSetExists)
            {
                return GetNotFoundResourceSet();
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, GetResourceStoreName + id, false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, GetResourcesStoreName, false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        #endregion

        #region Private methods

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

        #endregion
    }
}
