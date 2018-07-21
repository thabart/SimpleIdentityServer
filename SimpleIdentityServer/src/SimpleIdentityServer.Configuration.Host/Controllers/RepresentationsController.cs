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
using SimpleIdentityServer.Configuration.Extensions;
using System.Threading.Tasks;
using WebApiContrib.Core.Concurrency;

namespace SimpleIdentityServer.Configuration.Controllers
{
    [Route(Constants.RouteValues.Representations)]
    public class RepresentationsController : Controller
    {
        private readonly IRepresentationManager _representationManager;

        public RepresentationsController(IRepresentationManager representationManager)
        {
            _representationManager = representationManager;
        }

        [HttpGet]
        [Authorize("display")]
        public async Task<ActionResult> Get()
        {
            var representations = await _representationManager.GetRepresentations().ConfigureAwait(false);
            return new OkObjectResult(representations.ToDtos());
        }

        [HttpDelete]
        [Authorize("manage")]
        public async Task<ActionResult> Delete()
        {
            await _representationManager.RemoveRepresentations().ConfigureAwait(false);
            return new NoContentResult();
        }
    }
}
