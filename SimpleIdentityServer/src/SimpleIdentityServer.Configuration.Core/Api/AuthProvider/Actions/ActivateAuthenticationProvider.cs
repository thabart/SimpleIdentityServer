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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Configuration.Core.Repositories;
using SimpleIdentityServer.Logging;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Core.Api.AuthProvider.Actions
{
    public interface IActivateAuthenticationProvider
    {
        Task<ActionResult> ExecuteAsync(string name, bool isEnabled);
    }


    internal class ActivateAuthenticationProvider : IActivateAuthenticationProvider
    {
        private readonly IAuthenticationProviderRepository _authenticationProviderRepository;

        private readonly IConfigurationEventSource _configurationEventSource;

        #region Constructor

        public ActivateAuthenticationProvider(
            IAuthenticationProviderRepository authenticationProviderRepository,
            IConfigurationEventSource configurationEventSource)
        {
            _authenticationProviderRepository = authenticationProviderRepository;
            _configurationEventSource = configurationEventSource;
        }

        #endregion

        #region Public methods

        public async Task<ActionResult> ExecuteAsync(string name, bool isEnabled)
        {
            _configurationEventSource.EnableAuthenticationProvider(name, isEnabled);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var authProvider = await _authenticationProviderRepository.GetAuthenticationProvider(name);
            if (authProvider == null)
            {
                return new NotFoundResult();
            }

            authProvider.IsEnabled = isEnabled;
            var isUpdated = await _authenticationProviderRepository.UpdateAuthenticationProvider(authProvider);
            if (isUpdated)
            {
                _configurationEventSource.FinishToEnableAuthenticationProvider(name, isEnabled);
                return new NoContentResult();
            }

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        #endregion
    }
}
