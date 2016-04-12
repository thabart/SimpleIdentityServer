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

using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Uma.Core.Api.ScopeController;
using SimpleIdentityServer.Uma.Host.DTOs.Requests;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Net;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Scope)]
    public class ScopesController
    {
        private readonly IScopeActions _scopeActions;

        #region Constructor

        public ScopesController(IScopeActions scopeActions)
        {
            _scopeActions = scopeActions;
        }

        #endregion

        #region Public methods

        [HttpGet]
        public ActionResult GetScopeIds()
        {
            var resourceSetIds = _scopeActions.GetScopes();
            return new HttpOkObjectResult(resourceSetIds);
        }

        [HttpGet("{id}")]
        public ActionResult GetScope(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = _scopeActions.GetScope(id);
            if (result == null)
            {
                return GetNotFoundScope();
            }

            return new HttpOkObjectResult(result.ToResponse());
        }

        [HttpPost]
        public ActionResult AddResourceSet([FromBody] PostScope postScope)
        {
            if (postScope == null)
            {
                throw new ArgumentNullException(nameof(postScope));
            }

            var parameter = postScope.ToParameter();
            _scopeActions.InsertScope(parameter);
            var response = new AddResourceSetResponse
            {
                Id = postScope.Id
            };
            return new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }

        [HttpPut]
        public ActionResult UpdateResourceSet([FromBody] PutScope putScope)
        {
            if (putScope == null)
            {
                throw new ArgumentNullException(nameof(putScope));
            }

            var parameter = putScope.ToParameter();
            var resourceSetExists = _scopeActions.UpdateScope(parameter);
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
        public ActionResult DeleleteResourceSet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var resourceSetExists = _scopeActions.DeleteScope(id);
            if (!resourceSetExists)
            {
                return GetNotFoundScope();
            }

            return new HttpStatusCodeResult((int)HttpStatusCode.NoContent);
        }

        #endregion

        #region Private methods

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

        #endregion
    }
}
