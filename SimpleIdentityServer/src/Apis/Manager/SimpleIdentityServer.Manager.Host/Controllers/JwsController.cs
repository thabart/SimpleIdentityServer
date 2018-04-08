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
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using SimpleIdentityServer.Manager.Core.Api.Jws;
using SimpleIdentityServer.Manager.Host.Extensions;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    [Route(Constants.EndPoints.Jws)]
    public class JwsController : Controller
    {
        private readonly IJwsActions _jwsActions;

        public JwsController(IJwsActions jwsActions)
        {
            _jwsActions = jwsActions;
        }

        [HttpGet]
        public async Task<JwsInformationResponse> GetJws([FromQuery] GetJwsRequest getJwsRequest)
        {
            if (getJwsRequest == null)
            {
                throw new ArgumentNullException(nameof(getJwsRequest));
            }

            var result = await _jwsActions.GetJwsInformation(getJwsRequest.ToParameter());
            return result.ToDto();
        }
        
        [HttpPost]
        public async Task<string> PostJws([FromBody] CreateJwsRequest createJwsRequest)
        {
            if (createJwsRequest == null)
            {
                throw new ArgumentNullException(nameof(createJwsRequest));
            }

            return await _jwsActions.CreateJws(createJwsRequest.ToParameter());
        }
    }
}
