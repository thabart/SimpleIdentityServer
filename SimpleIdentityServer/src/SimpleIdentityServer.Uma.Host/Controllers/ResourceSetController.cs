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

using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Uma.Core.Api.ResourceSetController;
using SimpleIdentityServer.Uma.Host.DTOs.Requests;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Net;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.ResourceSet)]
    public class ResourceSetController
    {
        private readonly IResourceSetActions _resourceSetActions;

        #region Constructor

        public ResourceSetController(IResourceSetActions resourceSetActions)
        {
            _resourceSetActions = resourceSetActions;
        }

        #endregion

        #region Public methods

        [HttpGet("{id}")]
        public ActionResult GetResourceSet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = _resourceSetActions.GetResourceSet(id);
            if (result == null)
            {
                return new HttpNotFoundResult();
            }

            return new HttpOkObjectResult(result);
        }

        [HttpPost]
        public ActionResult AddResourceSet([FromBody] PostResourceSet postResourceSet)
        {
            if (postResourceSet == null)
            {
                throw new ArgumentNullException(nameof(postResourceSet));
            }

            var parameter = postResourceSet.ToParameter();
            var result = _resourceSetActions.AddResourceSet(parameter);
            var response = new AddResourceSetResponse
            {
                Id = result
            };
            return new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }

        [HttpPut]
        public ActionResult UpdateResourceSet([FromBody] PutResourceSet putResourceSet)
        {
            if (putResourceSet == null)
            {
                throw new ArgumentNullException(nameof(putResourceSet));
            }

            var parameter = putResourceSet.ToParameter();
            var result = _resourceSetActions.UpdateResourceSet(parameter);
            if (result == HttpStatusCode.OK)
            {
                var response = new UpdateSetResponse
                {
                    Id = putResourceSet.Id
                };

                return new ObjectResult(response)
                {
                    StatusCode = (int)result
                };
            }

            return new HttpStatusCodeResult((int)result);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleleteResourceSet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var code = _resourceSetActions.RemoveResourceSet(id);
            return new HttpStatusCodeResult((int)code);
        }

        #endregion
    }
}
