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
using SimpleIdentityServer.Configuration.Core.Api.Setting;
using SimpleIdentityServer.Configuration.Core.Errors;
using SimpleIdentityServer.Configuration.Core.Exceptions;
using SimpleIdentityServer.Configuration.DTOs.Requests;
using SimpleIdentityServer.Configuration.DTOs.Responses;
using SimpleIdentityServer.Configuration.Extensions;
using SimpleIdentityServer.Configuration.Host.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Configuration.Controllers
{
    [Route(Constants.RouteValues.Setting)]
    public class SettingController : Controller
    {
        private readonly ISettingActions _settingActions;

        public SettingController(ISettingActions settingActions)
        {
            _settingActions = settingActions;
        }

        [HttpGet]
        public List<SettingResponse> GetAll()
        {
            return _settingActions.GetSettings().ToDtos();
        }

        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var setting = _settingActions.GetSetting(id);
            if (setting == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(setting.ToDto());
        }

        [HttpPost]
        public ActionResult Get([FromBody] GetSettingsRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var settings = _settingActions.BulkGetSettings(request.ToParameter()).Select(s => s.ToDto());
            return new OkObjectResult(settings);
        }

        [HttpDelete("{id}")]
        [Authorize("manage")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!_settingActions.DeleteSetting(id))
            {
                return new NotFoundResult();
            }

            return new NoContentResult();
        }

        [HttpPut]
        [Authorize("manage")]
        public ActionResult Put([FromBody] UpdateSettingRequest updateSettingRequest)
        {
            if (updateSettingRequest == null)
            {
                throw new ArgumentNullException(nameof(updateSettingRequest));
            }

            if (!_settingActions.UpdateSetting(updateSettingRequest.ToParameter()))
            {
                return new NotFoundResult();
            }

            return new NoContentResult();
        }

        [HttpPut("bulk")]
        [Authorize("manage")]
        public ActionResult Put([FromBody] IEnumerable<UpdateSettingRequest> updateSettingRequests)
        {
            if (updateSettingRequests == null)
            {
                throw new ArgumentNullException(nameof(updateSettingRequests));
            }

            ;
            if (!_settingActions.BulkUpdateSettings(updateSettingRequests.Select(s => s.ToParameter())))
            {
                throw new IdentityConfigurationException(ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.BulkUpdateSettingOperationFailed);
            }

            return new NoContentResult();
        }
    }
}
