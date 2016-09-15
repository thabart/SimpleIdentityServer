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
        #region Fields

        private const string ScopesStoreName = "Scopes";

        private const string ScopeStoreName = "Scope_";

        private readonly IScopeActions _scopeActions;

        private readonly IRepresentationManager _representationManager;

        #endregion

        #region Constructor

        public ScopesController(
            IScopeActions scopeActions,
            IRepresentationManager representationManager)
        {
            _scopeActions = scopeActions;
            _representationManager = representationManager;
        }

        #endregion

        #region Public methods

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

            var result = _scopeActions.GetScopes().ToDtos();
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

            var result = _scopeActions.GetScope(id).ToDto();
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

            _scopeActions.DeleteScope(id);
            await _representationManager.AddOrUpdateRepresentationAsync(this, ScopeStoreName + id, false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, ScopesStoreName, false);
            return new NoContentResult();
        }

        #endregion
    }
}
