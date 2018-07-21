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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Manager.Core.Api.Clients;
using SimpleIdentityServer.Manager.Host.DTOs.Requests;
using SimpleIdentityServer.Manager.Host.DTOs.Responses;
using SimpleIdentityServer.Manager.Host.Extensions;
using System;
using System.Threading.Tasks;
using WebApiContrib.Core.Concurrency;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    [Route(Constants.EndPoints.Clients)]
    public class ClientsController : Controller
    {
        public const string GetClientsStoreName = "GetClients";
        public const string GetClientStoreName = "GetClient_";
        private readonly IClientActions _clientActions;
        private readonly IRepresentationManager _representationManager;

        public ClientsController(
            IClientActions clientActions,
            IRepresentationManager representationManager)
        {
            _clientActions = clientActions;
            _representationManager = representationManager;
        }
        
        [HttpGet]
        [Authorize("manager")]
        public async Task<ActionResult> GetAll()
        {
            if (!await _representationManager.CheckRepresentationExistsAsync(this, GetClientsStoreName).ConfigureAwait(false))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result =  (await _clientActions.GetClients().ConfigureAwait(false)).ToDtos();
            await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientsStoreName).ConfigureAwait(false);
            return new OkObjectResult(result);
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public async Task<ActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!await _representationManager.CheckRepresentationExistsAsync(this, GetClientStoreName + id).ConfigureAwait(false))
            {
                return new ContentResult
                {
                    StatusCode = 412
                };
            }

            var result = (await _clientActions.GetClient(id).ConfigureAwait(false)).ToClientResponseDto();
            await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientStoreName + id).ConfigureAwait(false);
            return new OkObjectResult(result);
        }

        [HttpDelete("{id}")]
        [Authorize("manager")]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!await _clientActions.DeleteClient(id).ConfigureAwait(false))
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientStoreName + id, false).ConfigureAwait(false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientsStoreName, false).ConfigureAwait(false);
            return new NoContentResult();
        }

        [HttpPut]
        [Authorize("manager")]
        public async Task<ActionResult> Put([FromBody] UpdateClientRequest updateClientRequest)
        {
            if (updateClientRequest == null)
            {
                throw new ArgumentNullException(nameof(updateClientRequest));
            }

            if (!await _clientActions.UpdateClient(updateClientRequest.ToParameter()).ConfigureAwait(false))
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientStoreName + updateClientRequest.ClientId, false).ConfigureAwait(false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientsStoreName, false).ConfigureAwait(false);
            return new NoContentResult();
        }

        [HttpPost]
        [Authorize("manager")]
        public async Task<ActionResult> Add([FromBody] ClientResponse client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var result = await _clientActions.AddClient(client.ToParameter()).ConfigureAwait(false);
            await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientsStoreName, false).ConfigureAwait(false);
            return new OkObjectResult(result);
        }
    }
}
