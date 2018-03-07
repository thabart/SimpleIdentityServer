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
using Microsoft.Net.Http.Headers;
using SimpleIdentityServer.Scim.Core.Results;
using System;

namespace SimpleIdentityServer.Scim.Host.Extensions
{
    public static class ControllerExtensions
    {
        public static ActionResult GetActionResult(this Controller controller, ApiActionResult result)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }


            if (!string.IsNullOrWhiteSpace(result.Location))
            {
                controller.HttpContext.Response.Headers[HeaderNames.Location] = result.Location;
            }

            if (result.Content != null)
            {
                var res = new ObjectResult(result.Content);
                res.StatusCode = result.StatusCode;
                return res;
            }

            return new StatusCodeResult(result.StatusCode.Value);
        }
    }
}
