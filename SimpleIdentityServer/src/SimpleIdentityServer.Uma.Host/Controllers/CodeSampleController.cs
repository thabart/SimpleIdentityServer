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
using SimpleIdentityServer.Uma.Core.Api.CodeSampleController;
using System;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.CodeSample)]
    public class CodeSampleController : Controller
    {
        private readonly ICodeSampleActions _codeSampleActions;

        #region Constructor

        public CodeSampleController(ICodeSampleActions codeSampleActions)
        {
            _codeSampleActions = codeSampleActions;
        }

        #endregion

        #region Public methods

        [HttpGet("backend/{id}")]
        public ActionResult GetBackend(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = _codeSampleActions.GetBackendCode(id);
            result.Position = 0;
            return new FileStreamResult(result, "application/zip")
            {
                FileDownloadName = "Backend.zip"
            };
        }

        [HttpGet("frontend/{id}")]
        public ActionResult GetFrontend(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = _codeSampleActions.GetFrontendCode(id);
            result.Position = 0;
            return new FileStreamResult(result, "application/zip")
            {
                FileDownloadName = "Frontend.zip"
            };
        }

        #endregion
    }
}
