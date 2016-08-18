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
using SimpleIdentityServer.Manager.Core.Api.Configuration;
using SimpleIdentityServer.Manager.Host.DTOs.Requests;
using SimpleIdentityServer.Manager.Host.DTOs.Responses;
using SimpleIdentityServer.Manager.Host.Extensions;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    public class ConfigurationController : Controller
    {
        #region Fields

        private readonly IConfigurationActions _configurationActions;

        #endregion

        #region Constructor

        public ConfigurationController(IConfigurationActions configurationActions)
        {
            _configurationActions = configurationActions;
        }

        #endregion

        #region Actions

        [HttpGet]
        [Authorize("manager")]
        public List<ConfigurationResponse> GetAll()
        {
            return _configurationActions.GetConfigurations().ToDtos();
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public ActionResult Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var configuration = _configurationActions.GetConfiguration(id);
            if (configuration == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(configuration.ToDto());
        }

        [HttpDelete("{id}")]
        [Authorize("manager")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!_configurationActions.DeleteConfiguration(id))
            {
                return new NotFoundResult();
            }

            return new NoContentResult();
        }

        [HttpPut]
        [Authorize("manager")]
        public ActionResult Put([FromBody] UpdateConfigurationRequest updateConfigurationRequest)
        {
            if (updateConfigurationRequest == null)
            {
                throw new ArgumentNullException(nameof(updateConfigurationRequest));
            }

            if(!_configurationActions.UpdateConfiguration(updateConfigurationRequest.ToParameter()))
            {
                return new NotFoundResult();
            }

            return new NoContentResult();
        }


        #endregion
    }
}
