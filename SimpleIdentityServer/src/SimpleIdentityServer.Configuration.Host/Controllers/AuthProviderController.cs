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
using SimpleIdentityServer.Configuration.Core.Api.AuthProvider;
using SimpleIdentityServer.Configuration.Core.Models;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Controllers
{
    [Route(Constants.RouteValues.AuthProvider)]
    public class AuthProviderController : Controller
    {
        private readonly IAuthProviderActions _authProviderActions;

        #region Constructor

        public AuthProviderController(IAuthProviderActions authProviderActions)
        {
            _authProviderActions = authProviderActions;
        }

        #endregion

        #region Public methods

        [HttpGet]
        [Authorize("display")]
        public async Task<ActionResult> Get()
        {
           return await _authProviderActions.GetAuthenticationProviders();
        }

        [HttpGet("{id}")]
        [Authorize("display")]
        public async Task<ActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await _authProviderActions.GetAuthenticationProvider(id);
        }

        [HttpGet("{id}/enable")]
        [Authorize("manage")]
        public async Task<ActionResult> GetEnable(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await _authProviderActions.EnableAuthenticationProvider(id);
        }

        [HttpGet("{id}/disable")]
        [Authorize("manage")]
        public async Task<ActionResult> GetDisable(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await _authProviderActions.DisableAuthenticationProvider(id);
        }

        [HttpPut]
        [Authorize("manage")]
        public async Task<ActionResult> Update([FromBody] AuthenticationProvider authenticationProvider)
        {
            if (authenticationProvider == null)
            {
                throw new ArgumentNullException(nameof(authenticationProvider));
            }

            return await _authProviderActions.UpdateAuthenticationProvider(authenticationProvider);
        }

        [HttpPost]
        [Authorize("manage")]
        public async Task<ActionResult> Add([FromBody] AuthenticationProvider authenticationProvider)
        {
            if (authenticationProvider == null)
            {
                throw new ArgumentNullException(nameof(authenticationProvider));
            }

            return await _authProviderActions.AddAuthenticationProvider(authenticationProvider);
        }

        [HttpDelete("{id}")]
        [Authorize("manage")]
        public async Task<ActionResult> Remove(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await _authProviderActions.DeleteAuthenticationProvider(id);
        }

        #endregion
    }
}
