#region copyright
// Copyright 2017 Habart Thierry
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

namespace SimpleIdentityServer.Scim.Startup.Controllers
{
    [Route(Constants.RoutePaths.BulkController)]
    public class BulkController : Controller
    {
        private readonly IBulkAction _bulkAction;

        public BulkController(IBulkAction bulkAction)
        {
            _bulkAction = bulkAction;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var result = await _bulkAction.Execute(jObj, GetLocationPattern()).ConfigureAwait(false);
            return this.GetActionResult(result);
        }

        private string GetLocationPattern()
        {
            return Request.GetAbsoluteUriWithVirtualPath() + "/{rootPath}";
        }
    }
}
