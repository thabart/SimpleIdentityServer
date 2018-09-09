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
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Common.DTOs;
using SimpleIdentityServer.Scim.Core.Apis;
using SimpleIdentityServer.Scim.Host.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApiContrib.Core.Concurrency;

namespace SimpleIdentityServer.Scim.Host.Controllers
{
    [Route(Core.Constants.RoutePaths.UsersController)]
    public class UsersController : Controller
    {
        private readonly IUsersAction _usersAction;
        private readonly IRepresentationManager _representationManager;
        private readonly string UsersName = "Users_{0}";

        public UsersController(
            IUsersAction usersAction,
            IRepresentationManager representationManager)
        {
            _usersAction = usersAction;
            _representationManager = representationManager;
        }

        [Authorize("scim_manage")]
        [HttpPost]
        public Task<ActionResult> Create([FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            return CreateUser(jObj);
        }

        [Authorize("scim_read")]
        [HttpGet("{id}")]
        public Task<ActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetUser(id);
        }

        [Authorize("scim_manage")]
        [HttpPatch("{id}")]
        public Task<ActionResult> Patch(string id, [FromBody] JObject jObj)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            return PatchUser(id, jObj);
        }

        [Authorize("scim_manage")]
        [HttpPut("{id}")]
        public Task<ActionResult> Update(string id, [FromBody] JObject jObj)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            return UpdateUser(id, jObj);
        }

        [Authorize("scim_manage")]
        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return DeleteUser(id);
        }

        [Authorize("scim_read")]
        [HttpGet]
        public async Task<ActionResult> SearchUsers()
        {
            var result = await _usersAction.SearchUsers(Request.Query, GetLocationPattern()).ConfigureAwait(false);
            return this.GetActionResult(result);
        }

        [Authorize("scim_read")]
        [HttpPost(".search")]
        public async Task<ActionResult> SearchUsers([FromBody] JObject jObj)
        {
            var result = await _usersAction.SearchUsers(jObj, GetLocationPattern()).ConfigureAwait(false);
            return this.GetActionResult(result);
        }

        #region Current user operations

        [HttpPost("Me")]
        [Authorize("authenticated")]
        public Task<ActionResult> CreateAuthenticatedUser()
        {
            var subject = GetSubject(User);
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Task.FromResult(GetMissingSubjectError());
            }

            var jObj = new JObject();
            jObj.Add(Common.Constants.ScimResourceNames.Schemas, new JArray(Common.Constants.SchemaUrns.User));
            jObj.Add(Common.Constants.IdentifiedScimResourceNames.ExternalId, subject);
            return CreateUser(jObj);
        }

        [HttpGet("Me")]
        [Authorize("authenticated")]
        public Task<ActionResult> GetAuthenticateUser()
        {
            var scimId = GetScimIdentifier(User);
            if (string.IsNullOrWhiteSpace(scimId))
            {
                return Task.FromResult(GetMissingScimIdentifierError());
            }

            return GetUser(scimId);
        }

        [HttpPatch("Me")]
        [Authorize("authenticated")]
        public Task<ActionResult> PatchAuthenticatedUser([FromBody] JObject jObj)
        {
            var scimId = GetScimIdentifier(User);
            if (string.IsNullOrWhiteSpace(scimId))
            {
                return Task.FromResult(GetMissingScimIdentifierError());
            }

            return PatchUser(scimId, jObj);
        }

        [HttpPut("Me")]
        [Authorize("authenticated")]
        public Task<ActionResult> UpdateAuthenticatedUser([FromBody] JObject jObj)
        {
            var scimId = GetScimIdentifier(User);
            if (string.IsNullOrWhiteSpace(scimId))
            {
                return Task.FromResult(GetMissingScimIdentifierError());
            }

            return UpdateUser(scimId, jObj);
        }
        
        [HttpDelete("Me")]
        [Authorize("authenticated")]
        public Task<ActionResult> DeleteAuthenticatedUser()
        {
            var scimId = GetScimIdentifier(User);
            if (string.IsNullOrWhiteSpace(scimId))
            {
                return Task.FromResult(GetMissingScimIdentifierError());
            }

            return DeleteUser(scimId);
        }

        #endregion

        #region Common user operations

        private async Task<ActionResult> CreateUser(JObject jObj)
        {
            var result = await _usersAction.AddUser(jObj, GetLocationPattern()).ConfigureAwait(false);
            if (result.IsSucceed())
            {
                await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(UsersName, result.Id), result.Version, true).ConfigureAwait(false);
            }

            return this.GetActionResult(result);
        }

        private async Task<ActionResult> GetUser(string id)
        {
            if (!await _representationManager.CheckRepresentationExistsAsync(this, string.Format(UsersName, id)).ConfigureAwait(false))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result = await _usersAction.GetUser(id, GetLocationPattern()).ConfigureAwait(false);
            if (result.IsSucceed())
            {
                await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(UsersName, result.Id), result.Version, true).ConfigureAwait(false);
            }

            return this.GetActionResult(result);
        }

        private async Task<ActionResult> PatchUser(string id, JObject jObj)
        {
            var result = await _usersAction.PatchUser(id, jObj, GetLocationPattern()).ConfigureAwait(false);
            if (result.IsSucceed())
            {
                await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(UsersName, result.Id), result.Version, true).ConfigureAwait(false);
            }

            return this.GetActionResult(result);
        }

        private async Task<ActionResult> UpdateUser(string id, JObject jObj)
        {
            var result = await _usersAction.UpdateUser(id, jObj, GetLocationPattern()).ConfigureAwait(false);
            if (result.IsSucceed())
            {
                await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(UsersName, result.Id), result.Version, true).ConfigureAwait(false);
            }

            return this.GetActionResult(result);
        }

        private async Task<ActionResult> DeleteUser(string id)
        {
            var result = await _usersAction.RemoveUser(id).ConfigureAwait(false);
            if (result.IsSucceed())
            {
                await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(UsersName, result.Id), result.Version, false).ConfigureAwait(false);
            }

            return this.GetActionResult(result);
        }

        #endregion

        #region Common operations

        private string GetLocationPattern()
        {
            return new Uri(new Uri(Request.GetAbsoluteUriWithVirtualPath()), Core.Constants.RoutePaths.UsersController).AbsoluteUri + "/{id}";
        }

        private static string GetSubject(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null || claimsPrincipal.Identity == null || !claimsPrincipal.Identity.IsAuthenticated  || claimsPrincipal.Claims == null)
            {
                return null;
            }

            var claim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "sub");
            if (claim == null)
            {
                return null;
            }

            return claim.Value;
        }

        private static string GetScimIdentifier(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null || claimsPrincipal.Identity == null || !claimsPrincipal.Identity.IsAuthenticated || claimsPrincipal.Claims == null)
            {
                return null;
            }

            var claim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "scim_id");
            if (claim == null)
            {
                return null;
            }

            return claim.Value;
        }

        private static ActionResult GetMissingSubjectError()
        {
            var error = new ErrorResponse
            {
                Detail = "the subject is missing",
                Schemas = new[] { Common.Constants.Messages.Error },
                Status = (int)HttpStatusCode.BadRequest
            };
            return new JsonResult(error)
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        private static ActionResult GetMissingScimIdentifierError()
        {
            var error = new ErrorResponse
            {
                Detail = "the scim_id is missing",
                Schemas = new[] { Common.Constants.Messages.Error },
                Status = (int)HttpStatusCode.BadRequest
            };
            return new JsonResult(error)
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        #endregion
    }
}
