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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using SimpleIdentityServer.Manager.Core.Api.Scopes;
using SimpleIdentityServer.Manager.Host.Extensions;
using System;
using System.Threading.Tasks;
using WebApiContrib.Core.Concurrency;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    [Route(Constants.EndPoints.Scopes)]
    public class ScopesController : Controller
    {
        private const string ScopesStoreName = "Scopes";
        private const string ScopeStoreName = "Scope_";
        private readonly IScopeActions _scopeActions;
        private readonly IRepresentationManager _representationManager;

        public ScopesController(
            IScopeActions scopeActions,
            IRepresentationManager representationManager)
        {
            _scopeActions = scopeActions;
            _representationManager = representationManager;
        }
        
        [HttpPost(".search")]
        [Authorize("manager")]
        public async Task<IActionResult> Search([FromBody] SearchScopesRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var parameter = request.ToSearchScopesParameter();
            var result = await _scopeActions.Search(parameter);
            return new OkObjectResult(result.ToDto());
        }

        [HttpGet]
        [Authorize("manager")]
        public async Task<ActionResult> GetAll()
        {
            if (!await _representationManager.CheckRepresentationExistsAsync(this, ScopesStoreName))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result = (await _scopeActions.GetScopes()).ToDtos();
            await _representationManager.AddOrUpdateRepresentationAsync(this, ScopesStoreName);
            return new OkObjectResult(result);
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public async Task<ActionResult> Get(string id)
        {
            if (!await _representationManager.CheckRepresentationExistsAsync(this, ScopeStoreName + id))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result = (await _scopeActions.GetScope(id)).ToDto();
            await _representationManager.AddOrUpdateRepresentationAsync(this, ScopeStoreName + id);
            return new OkObjectResult(result);
        }

        [HttpDelete("{id}")]
        [Authorize("manager")]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            await _scopeActions.DeleteScope(id);
            await _representationManager.AddOrUpdateRepresentationAsync(this, ScopeStoreName + id, false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, ScopesStoreName, false);
            return new NoContentResult();
        }

        [HttpPost]
        [Authorize("manager")]
        public async Task<ActionResult> Add([FromBody] ScopeResponse request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!await _scopeActions.AddScope(request.ToParameter()))
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, ScopesStoreName, false);
            return new NoContentResult();
        }
        
        [HttpPut]
        [Authorize("manager")]
        public async Task<ActionResult> Update([FromBody] ScopeResponse request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!await _scopeActions.UpdateScope(request.ToParameter()))
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, ScopeStoreName + request.Name, false);
            return new NoContentResult();
        }
    }
}
