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
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Manager.Core.Api.ResourceOwners;
using SimpleIdentityServer.Manager.Host.DTOs.Requests;
using SimpleIdentityServer.Manager.Host.DTOs.Responses;
using SimpleIdentityServer.Manager.Host.Extensions;
using System;
using System.Threading.Tasks;
using WebApiContrib.Core.Concurrency;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    [Route(Constants.EndPoints.ResourceOwners)]
    public class ResourceOwnersController : Controller
    {
        private readonly IResourceOwnerActions _resourceOwnerActions;
        private readonly IRepresentationManager _representationManager;

        public ResourceOwnersController(
            IResourceOwnerActions resourceOwnerActions,
            IRepresentationManager representationManager)
        {
            _resourceOwnerActions = resourceOwnerActions;
            _representationManager = representationManager;
        }

        [HttpGet]
        [Authorize("manager")]
        public async Task<ActionResult> Get()
        {
            if (!await _representationManager.CheckRepresentationExistsAsync(this, StoreNames.GetResourceOwners).ConfigureAwait(false))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var content = (await _resourceOwnerActions.GetResourceOwners().ConfigureAwait(false)).ToDtos();
            await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwners).ConfigureAwait(false);
            return new OkObjectResult(content);
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public async Task<ActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            
            if (!await _representationManager.CheckRepresentationExistsAsync(this, StoreNames.GetResourceOwner + id).ConfigureAwait(false))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var content = (await _resourceOwnerActions.GetResourceOwner(id).ConfigureAwait(false)).ToDto();
            await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwner + id).ConfigureAwait(false);
            return new OkObjectResult(content);
        }

        [HttpDelete("{id}")]
        [Authorize("manager")]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            await _resourceOwnerActions.Delete(id).ConfigureAwait(false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwner + id, false).ConfigureAwait(false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwners, false).ConfigureAwait(false);
            return new NoContentResult();
        }

        [HttpPut]
        [Authorize("manager")]
        public async Task<ActionResult> Update([FromBody] ResourceOwnerResponse resourceOwnerResponse)
        {
            if (resourceOwnerResponse == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerResponse));
            }

            await _resourceOwnerActions.UpdateResourceOwner(resourceOwnerResponse.ToParameter()).ConfigureAwait(false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwner + resourceOwnerResponse.Login, false).ConfigureAwait(false);
            return new NoContentResult();
        }

        [HttpPost]
        [Authorize("manager")]
        public async Task<ActionResult> Add([FromBody] AddResourceOwnerRequest addResourceOwnerRequest)
        {
            if (addResourceOwnerRequest == null)
            {
                throw new ArgumentNullException(nameof(addResourceOwnerRequest));
            }

            await _resourceOwnerActions.Add(addResourceOwnerRequest.ToParameter()).ConfigureAwait(false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwners, false).ConfigureAwait(false);
            return new NoContentResult();
        }
    }
}
