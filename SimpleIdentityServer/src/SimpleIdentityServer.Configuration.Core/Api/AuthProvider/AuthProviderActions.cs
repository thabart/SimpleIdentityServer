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

using System.Threading.Tasks;
using SimpleIdentityServer.Configuration.Core.Api.AuthProvider.Actions;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Configuration.Core.Models;

namespace SimpleIdentityServer.Configuration.Core.Api.AuthProvider
{
    public interface IAuthProviderActions
    {
        Task<ActionResult> GetAuthenticationProvider(string name);

        Task<ActionResult> GetAuthenticationProviders();

        Task<ActionResult> EnableAuthenticationProvider(string name);

        Task<ActionResult> DisableAuthenticationProvider(string name);

        Task<ActionResult> UpdateAuthenticationProvider(AuthenticationProvider authenticationProvider);

        Task<ActionResult> AddAuthenticationProvider(AuthenticationProvider authenticationProvider);
    }

    internal class AuthProviderActions : IAuthProviderActions
    {
        private readonly IGetAuthenticationProvider _getAuthenticationProvider;

        private readonly IGetAuthenticationProviders _getAuthenticationProviders;

        private readonly IActivateAuthenticationProvider _activateAuthenticationProvider;

        private readonly IUpdateAuthenticationProvider _updateAuthenticationProvider;

        private readonly IAddAuthenticationProviderAction _addAuthenticationProviderAction;

        #region Constructor

        public AuthProviderActions(
            IGetAuthenticationProvider getAuthenticationProvider,
            IGetAuthenticationProviders getAuthenticationProviders,
            IActivateAuthenticationProvider activateAuthenticationProvider,
            IUpdateAuthenticationProvider updateAuthenticationProvider,
            IAddAuthenticationProviderAction addAuthenticationProviderAction)
        {
            _getAuthenticationProvider = getAuthenticationProvider;
            _getAuthenticationProviders = getAuthenticationProviders;
            _activateAuthenticationProvider = activateAuthenticationProvider;
            _updateAuthenticationProvider = updateAuthenticationProvider;
            _addAuthenticationProviderAction = addAuthenticationProviderAction;
        }

        #endregion

        #region Public methods

        public async Task<ActionResult> GetAuthenticationProvider(string name)
        {
            return await _getAuthenticationProvider.ExecuteAsync(name);
        }

        public async Task<ActionResult> GetAuthenticationProviders()
        {
            return await _getAuthenticationProviders.ExecuteAsync();
        }

        public async Task<ActionResult> EnableAuthenticationProvider(string name)
        {
            return await _activateAuthenticationProvider.ExecuteAsync(name, true);
        }

        public async Task<ActionResult> DisableAuthenticationProvider(string name)
        {
            return await _activateAuthenticationProvider.ExecuteAsync(name, false);
        }

        public async Task<ActionResult> UpdateAuthenticationProvider(AuthenticationProvider authenticationProvider)
        {
            return await _updateAuthenticationProvider.ExecuteAsync(authenticationProvider);
        }

        public async Task<ActionResult> AddAuthenticationProvider(AuthenticationProvider authenticationProvider)
        {
            return await _addAuthenticationProviderAction.ExecuteAsync(authenticationProvider);
        }

        #endregion
    }
}
