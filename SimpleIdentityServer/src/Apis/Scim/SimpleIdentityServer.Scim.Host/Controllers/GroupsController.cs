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
using SimpleIdentityServer.Scim.Core.Apis;
using SimpleIdentityServer.Scim.Host.Extensions;
using System;
using System.Threading.Tasks;
using WebApiContrib.Core.Concurrency;

namespace SimpleIdentityServer.Scim.Host.Controllers
{
    [Route(Core.Constants.RoutePaths.GroupsController)]
    public class GroupsController : Controller
    {
        private readonly IGroupsAction _groupsAction;
        private readonly IRepresentationManager _representationManager;
        private readonly string GroupsName = "Groups_{0}";

        public GroupsController(
            IGroupsAction groupsAction,
            IRepresentationManager representationManager)
        {
            _groupsAction = groupsAction;
            _representationManager = representationManager;
        }

        [Authorize("scim_manage")]
        [HttpPost]
        public async Task<ActionResult> AddGroup([FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var result = await _groupsAction.AddGroup(jObj, GetLocationPattern()).ConfigureAwait(false);
            if (result.IsSucceed())
            {
                await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(GroupsName, result.Id), result.Version, true).ConfigureAwait(false);
            }

            return this.GetActionResult(result);
        }

        [Authorize("scim_read")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetGroup(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            
            if (!await _representationManager.CheckRepresentationExistsAsync(this, string.Format(GroupsName, id)).ConfigureAwait(false))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result = await _groupsAction.GetGroup(id, GetLocationPattern(), Request.Query).ConfigureAwait(false);
            if (result.IsSucceed())
            {
                await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(GroupsName, result.Id), result.Version, true).ConfigureAwait(false);
            }

            return this.GetActionResult(result);
        }

        [Authorize("scim_read")]
        [HttpGet]
        public async Task<ActionResult> SearchGroups()
        {
            var result = await _groupsAction.SearchGroups(Request.Query, GetLocationPattern()).ConfigureAwait(false);
            return this.GetActionResult(result);
        }

        [Authorize("scim_read")]
        [HttpPost(".search")]
        public async Task<ActionResult> SearchGroups([FromBody] JObject jObj)
        {
            var result = await _groupsAction.SearchGroups(jObj, GetLocationPattern()).ConfigureAwait(false);
            return this.GetActionResult(result);
        }

        [Authorize("scim_manage")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteGroup(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = await _groupsAction.RemoveGroup(id).ConfigureAwait(false);
            if (result.IsSucceed())
            {
                await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(GroupsName, result.Id), false).ConfigureAwait(false);
            }

            return this.GetActionResult(result);
        }

        [Authorize("scim_manage")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateGroup(string id, [FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var result = await _groupsAction.UpdateGroup(id, jObj, GetLocationPattern()).ConfigureAwait(false);
            if (result.IsSucceed())
            {
                await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(GroupsName, result.Id), result.Version, true).ConfigureAwait(false);
            }

            return this.GetActionResult(result);
        }

        [Authorize("scim_manage")]
        [HttpPatch("{id}")]
        public async Task<ActionResult> PatchGroup(string id, [FromBody] JObject jObj)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var result = await _groupsAction.PatchGroup(id, jObj, GetLocationPattern()).ConfigureAwait(false);
            if (result.IsSucceed())
            {
                await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(GroupsName, result.Id), result.Version, true).ConfigureAwait(false);
            }

            return this.GetActionResult(result);
        }

        private string GetLocationPattern()
        {
            return new Uri(new Uri(Request.GetAbsoluteUriWithVirtualPath()), Core.Constants.RoutePaths.GroupsController).AbsoluteUri + "/{id}";
        }
    }
}
