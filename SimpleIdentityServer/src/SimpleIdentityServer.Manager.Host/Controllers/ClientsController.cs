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
using SimpleIdentityServer.Manager.Core.Api.Clients;
using SimpleIdentityServer.Manager.Host.DTOs.Requests;
using SimpleIdentityServer.Manager.Host.DTOs.Responses;
using SimpleIdentityServer.Manager.Host.Extensions;
using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    [Route(Constants.EndPoints.Clients)]
    public class ClientsController : Controller
    {
        private readonly IClientActions _clientActions;

        #region Constructor

        public ClientsController(IClientActions clientActions)
        {
            _clientActions = clientActions;
        }

        #endregion

        #region Public methods

        [HttpGet]
        [Authorize("manager")]
        public List<ClientInformationResponse> GetAll()
        {
            var result =  _clientActions.GetClients().ToDtos();
            return result;
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public ClientResponse Get(string id)
        {
            return _clientActions.GetClient(id).ToClientResponseDto();
        }

        [HttpDelete("{id}")]
        [Authorize("manager")]
        public bool Delete(string id)
        {
            return _clientActions.DeleteClient(id);
        }

        [HttpPut]
        [Authorize("manager")]
        public bool Put([FromBody] UpdateClientRequest updateClientRequest)
        {
            return _clientActions.UpdateClient(updateClientRequest.ToParameter());
        }

        #endregion
    }
}
