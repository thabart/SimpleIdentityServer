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
using SimpleIdentityServer.Scim.Core.Apis;
using SimpleIdentityServer.Scim.Startup.Extensions;
using System;

namespace SimpleIdentityServer.Scim.Startup.Controllers
{
    [Route(Constants.RoutePaths.GroupsController)]
    public class GroupsController : Controller
    {
        private readonly IGroupsAction _groupsAction;

        public GroupsController(IGroupsAction groupsAction)
        {
            _groupsAction = groupsAction;
        }

        [HttpPost]
        public ActionResult AddGroup([FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var result = _groupsAction.AddGroup(jObj, GetLocationPattern());
            return this.GetActionResult(result);
        }

        [HttpGet("{id}")]
        public ActionResult GetGroup(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = _groupsAction.GetGroup(id, GetLocationPattern(), Request.Query);
            return this.GetActionResult(result);
        }

        [HttpGet]
        public ActionResult SearchGroups()
        {
            var result = _groupsAction.SearchGroups(Request.Query);
            return this.GetActionResult(result);
        }

        [HttpPost(".search")]
        public ActionResult SearchGroups([FromBody] JObject jObj)
        {
            var result = _groupsAction.SearchGroups(jObj);
            return this.GetActionResult(result);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteGroup(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = _groupsAction.RemoveGroup(id);
            return this.GetActionResult(result);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateGroup(string id, [FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var result = _groupsAction.UpdateGroup(id, jObj, GetLocationPattern());
            return this.GetActionResult(result);
        }

        [HttpPatch("{id}")]
        public ActionResult PatchGroup(string id, [FromBody] JObject jObj)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var result = _groupsAction.PatchGroup(id, jObj, GetLocationPattern());
            return this.GetActionResult(result);
        }

        private string GetLocationPattern()
        {
            return new Uri(new Uri(Request.GetAbsoluteUriWithVirtualPath()), Constants.RoutePaths.GroupsController).AbsoluteUri + "/{id}";
        }
    }
}
