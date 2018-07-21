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

using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Uma.Common.DTOs;
using SimpleIdentityServer.Uma.Core.Api.ScopeController;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Scope)]
    public class ScopesController : Controller
    {
        private readonly IScopeActions _scopeActions;

        public ScopesController(IScopeActions scopeActions)
        {
            _scopeActions = scopeActions;
        }

        [HttpGet]
        public async Task<ActionResult> GetScopes()
        {
            var resourceSetIds = await _scopeActions.GetScopes().ConfigureAwait(false);
            return new OkObjectResult(resourceSetIds);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetScope(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = await _scopeActions.GetScope(id).ConfigureAwait(false);
            if (result == null)
            {
                return GetNotFoundScope();
            }

            return new OkObjectResult(result.ToResponse());
        }

        [HttpPost]
        public async Task<ActionResult> AddScope([FromBody] PostScope postScope)
        {
            if (postScope == null)
            {
                throw new ArgumentNullException(nameof(postScope));
            }

            var parameter = postScope.ToParameter();
            await _scopeActions.InsertScope(parameter).ConfigureAwait(false);
            var response = new AddScopeResponse
            {
                Id = postScope.Id
            };
            return new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }

        [HttpPut]
        public async Task<ActionResult> UpdateScope([FromBody] PutScope putScope)
        {
            if (putScope == null)
            {
                throw new ArgumentNullException(nameof(putScope));
            }

            var parameter = putScope.ToParameter();
            var resourceSetExists = await _scopeActions.UpdateScope(parameter).ConfigureAwait(false);
            if (!resourceSetExists)
            {
                return GetNotFoundScope();
            }

            var response = new UpdateScopeResponse
            {
                Id = putScope.Id
            };

            return new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteScope(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var resourceSetExists = await _scopeActions.DeleteScope(id).ConfigureAwait(false);
            if (!resourceSetExists)
            {
                return GetNotFoundScope();
            }

            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        private static ActionResult GetNotFoundScope()
        {
            var errorResponse = new ErrorResponse
            {
                Error = Constants.ErrorCodes.NotFound,
                ErrorDescription = Constants.ErrorDescriptions.ScopeNotFound
            };

            return new ObjectResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.NotFound
            };
        }
    }
}
