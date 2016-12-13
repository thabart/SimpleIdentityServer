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
        Task<ActionResult> DeleteAuthenticationProvider(string name);
    }

    internal class AuthProviderActions : IAuthProviderActions
    {
        private readonly IGetAuthenticationProvider _getAuthenticationProvider;
        private readonly IGetAuthenticationProviders _getAuthenticationProviders;
        private readonly IActivateAuthenticationProvider _activateAuthenticationProvider;
        private readonly IUpdateAuthenticationProvider _updateAuthenticationProvider;
        private readonly IAddAuthenticationProviderAction _addAuthenticationProviderAction;
        private readonly IRemoveAuthenticationProviderAction _removeAuthenticationProviderAction;

        public AuthProviderActions(
            IGetAuthenticationProvider getAuthenticationProvider,
            IGetAuthenticationProviders getAuthenticationProviders,
            IActivateAuthenticationProvider activateAuthenticationProvider,
            IUpdateAuthenticationProvider updateAuthenticationProvider,
            IAddAuthenticationProviderAction addAuthenticationProviderAction,
            IRemoveAuthenticationProviderAction removeAuthenticationProviderAction)
        {
            _getAuthenticationProvider = getAuthenticationProvider;
            _getAuthenticationProviders = getAuthenticationProviders;
            _activateAuthenticationProvider = activateAuthenticationProvider;
            _updateAuthenticationProvider = updateAuthenticationProvider;
            _addAuthenticationProviderAction = addAuthenticationProviderAction;
            _removeAuthenticationProviderAction = removeAuthenticationProviderAction;
        }

        public Task<ActionResult> GetAuthenticationProvider(string name)
        {
            return _getAuthenticationProvider.ExecuteAsync(name);
        }

        public Task<ActionResult> GetAuthenticationProviders()
        {
            return _getAuthenticationProviders.ExecuteAsync();
        }

        public Task<ActionResult> EnableAuthenticationProvider(string name)
        {
            return _activateAuthenticationProvider.ExecuteAsync(name, true);
        }

        public Task<ActionResult> DisableAuthenticationProvider(string name)
        {
            return _activateAuthenticationProvider.ExecuteAsync(name, false);
        }

        public Task<ActionResult> UpdateAuthenticationProvider(AuthenticationProvider authenticationProvider)
        {
            return _updateAuthenticationProvider.ExecuteAsync(authenticationProvider);
        }

        public Task<ActionResult> AddAuthenticationProvider(AuthenticationProvider authenticationProvider)
        {
            return _addAuthenticationProviderAction.ExecuteAsync(authenticationProvider);
        }

        public Task<ActionResult> DeleteAuthenticationProvider(string name)
        {
            return _removeAuthenticationProviderAction.ExecuteAsync(name);
        }
    }
}
