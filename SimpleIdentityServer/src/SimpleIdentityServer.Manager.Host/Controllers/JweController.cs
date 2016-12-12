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
using SimpleIdentityServer.Manager.Core.Api.Jwe;
using SimpleIdentityServer.Manager.Host.DTOs.Requests;
using SimpleIdentityServer.Manager.Host.DTOs.Responses;
using SimpleIdentityServer.Manager.Host.Extensions;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    [Route(Constants.EndPoints.Jwe)]
    public class JweController : Controller
    {
        private readonly IJweActions _jweActions;

        public JweController(IJweActions jweActions)
        {
            _jweActions = jweActions;
        }
                
        [HttpGet]
        public async Task<JweInformationResponse> GetJwe([FromQuery] GetJweRequest getJweRequest)
        {
            if (getJweRequest == null)
            {
                throw new ArgumentNullException(nameof(getJweRequest));
            }

            var result = await _jweActions.GetJweInformation(getJweRequest.ToParameter());
            return result.ToDto();
        }

        [HttpPost]
        public async Task<string> PostJwe([FromBody] CreateJweRequest createJweRequest)
        {
            if (createJweRequest == null)
            {
                throw new ArgumentNullException(nameof(createJweRequest));
            }

            return await _jweActions.CreateJwe(createJweRequest.ToParameter());
        }
    }
}
