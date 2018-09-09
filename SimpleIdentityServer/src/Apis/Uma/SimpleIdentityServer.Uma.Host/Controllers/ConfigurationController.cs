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
using SimpleIdentityServer.Uma.Core.Api.ConfigurationController;
using SimpleIdentityServer.Uma.Host.Extensions;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Configuration)]
    public class ConfigurationController : Controller
    {
        private readonly IConfigurationActions _configurationActions;
        
        public ConfigurationController(IConfigurationActions configurationActions)
        {
            _configurationActions = configurationActions;
        }

        [HttpGet]
        public async Task<ActionResult> GetConfiguration()
        {
            var result = (await _configurationActions.GetConfiguration().ConfigureAwait(false)).ToResponse();
            return new OkObjectResult(result);
        }
    }
}
