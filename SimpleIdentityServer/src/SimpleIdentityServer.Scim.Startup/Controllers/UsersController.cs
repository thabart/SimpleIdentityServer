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

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core;
using SimpleIdentityServer.Scim.Core.Apis;
using SimpleIdentityServer.Scim.Startup.Extensions;
using System;
using System.Threading.Tasks;
using WebApiContrib.Core.Concurrency;

namespace SimpleIdentityServer.Scim.Startup.Controllers
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

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var result = _usersAction.AddUser(jObj, GetLocationPattern());
            await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(UsersName, result.Id), result.Version, true);
            return this.GetActionResult(result);
        }

        [HttpPatch("{id}")]
        public ActionResult PatchUser(string id, [FromBody] JObject jObj)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var result = _usersAction.PatchUser(id, jObj, GetLocationPattern());
            return this.GetActionResult(result);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateUser(string id, [FromBody] JObject jObj)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var result = _usersAction.UpdateUser(id, jObj, GetLocationPattern());
            return this.GetActionResult(result);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = _usersAction.RemoveUser(id);
            return this.GetActionResult(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!await _representationManager.CheckRepresentationExistsAsync(this, string.Format(UsersName, id)))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result = _usersAction.GetUser(id, GetLocationPattern());
            await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(UsersName, result.Version));
            return this.GetActionResult(result);
        }

        [HttpGet]
        public ActionResult SearchUsers()
        {
            var result = _usersAction.SearchUsers(Request.Query, GetLocationPattern());
            return this.GetActionResult(result);
        }

        [HttpPost(".search")]
        public ActionResult SearchUsers([FromBody] JObject jObj)
        {
            var result = _usersAction.SearchUsers(jObj, GetLocationPattern());
            return this.GetActionResult(result);
        }

        private string GetLocationPattern()
        {
            return new Uri(new Uri(Request.GetAbsoluteUriWithVirtualPath()), Core.Constants.RoutePaths.UsersController).AbsoluteUri + "/{id}";
        }
    }
}
