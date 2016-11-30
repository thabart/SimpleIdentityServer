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

using System;
using SimpleIdentityServer.Host.DTOs.Response;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Core.Api.Registration;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Host;
using Microsoft.AspNetCore.Mvc;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    [Route(Constants.EndPoints.Registration)]
    public class RegistrationController : Controller
    {
        private readonly IRegistrationActions _registerActions;

        public RegistrationController(IRegistrationActions registerActions)
        {
            _registerActions = registerActions;
        }

        [HttpPost]
        public RegistrationResponse Post([FromBody] ClientResponse client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            
            return _registerActions.PostRegistration(client.ToParameter());
        }
    }
}