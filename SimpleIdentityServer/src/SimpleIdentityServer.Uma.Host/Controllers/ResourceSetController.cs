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
using System.Collections.Generic;

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

        [HttpGet]
        public List<string> GetResourceSets()
        {
            return null;
        }

        [HttpGet("{id}")]
        public ResourceSetResponse GetResourceSet()
        {
            return null;
        }

        [HttpPost]
        public AddResourceSetResponse AddResourceSet([FromBody] PostResourceSet postResourceSet)
        {
            if (postResourceSet == null)
            {
                throw new ArgumentNullException(nameof(postResourceSet));
            }

            var parameter = postResourceSet.ToParameter();
            var result = _resourceSetActions.AddResourceSet(parameter);
            return new AddResourceSetResponse
            {
                Id = result
            };
        }

        [HttpPut]
        public UpdateSetResponse UpdateResourceSet()
        {
            return null;
        }

        #endregion
    }
}
