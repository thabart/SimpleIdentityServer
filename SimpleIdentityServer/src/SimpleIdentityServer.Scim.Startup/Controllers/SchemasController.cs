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
using SimpleIdentityServer.Scim.Core;
using SimpleIdentityServer.Scim.Core.Stores;

namespace SimpleIdentityServer.Scim.Startup.Controllers
{
    [Route(Constants.RoutePaths.SchemasController)]
    public class SchemasController : Controller
    {
        private readonly ISchemaStore _schemaStore;

        public SchemasController(ISchemaStore schemaStore)
        {
            _schemaStore = schemaStore;
        }

        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            return new OkObjectResult(_schemaStore.GetSchema(id));
        }

        [HttpGet]
        public ActionResult All()
        {
            return new OkObjectResult(_schemaStore.GetSchemas());
        }
    }
}
