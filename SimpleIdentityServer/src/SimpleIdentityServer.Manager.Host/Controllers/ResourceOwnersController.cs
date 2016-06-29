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
using SimpleIdentityServer.Manager.Core.Api.ResourceOwners;
using SimpleIdentityServer.Manager.Host.DTOs.Requests;
using SimpleIdentityServer.Manager.Host.Extensions;
using System;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    [Route(Constants.EndPoints.ResourceOwners)]
    public class ResourceOwnersController
    {
        #region Fields

        private readonly IResourceOwnerActions _resourceOwnerActions;

        #endregion

        #region Constructor

        public ResourceOwnersController(IResourceOwnerActions resourceOwnerActions)
        {
            _resourceOwnerActions = resourceOwnerActions;
        }

        #endregion

        #region Public methods

        [HttpGet]
        public ActionResult Get()
        {
            var content = _resourceOwnerActions.GetResourceOwners().ToDtos();
            return new OkObjectResult(content);
        }

        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var content = _resourceOwnerActions.GetResourceOwner(id).ToDto();
            return new OkObjectResult(content);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            _resourceOwnerActions.Delete(id);
            return new NoContentResult();
        }

        [HttpPut]
        public ActionResult Update([FromBody] UpdateResourceOwnerRequest updateResourceOwnerRequest)
        {
            if (updateResourceOwnerRequest == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerRequest));
            }

            _resourceOwnerActions.UpdateResourceOwner(updateResourceOwnerRequest.ToParameter());
            return new NoContentResult();
        }

        #endregion
    }
}
