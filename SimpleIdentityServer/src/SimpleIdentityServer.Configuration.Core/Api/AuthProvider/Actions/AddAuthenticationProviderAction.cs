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
using Newtonsoft.Json;
using SimpleIdentityServer.Configuration.Core.Errors;
using SimpleIdentityServer.Configuration.Core.Exceptions;
using SimpleIdentityServer.Configuration.Core.Models;
using SimpleIdentityServer.Configuration.Core.Repositories;
using SimpleIdentityServer.Logging;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Core.Api.AuthProvider.Actions
{
    public interface IAddAuthenticationProviderAction
    {
        Task<ActionResult> ExecuteAsync(AuthenticationProvider authenticationProvider);
    }

    internal class AddAuthenticationProviderAction : IAddAuthenticationProviderAction
    {
        #region Fields

        private readonly IAuthenticationProviderRepository _authenticationProviderRepository;

        private readonly IConfigurationEventSource _configurationEventSource;

        #endregion

        #region Constructor

        public AddAuthenticationProviderAction(
            IAuthenticationProviderRepository authenticationProviderRepository,
            IConfigurationEventSource configurationEventSource)
        {
            _authenticationProviderRepository = authenticationProviderRepository;
            _configurationEventSource = configurationEventSource;
        }

        #endregion

        #region Public methods

        public async Task<ActionResult> ExecuteAsync(AuthenticationProvider authenticationProvider)
        {
            var json = authenticationProvider == null ? string.Empty : JsonConvert.SerializeObject(authenticationProvider);
            _configurationEventSource.StartToAddAuthenticationProvider(json);
            if (authenticationProvider == null)
            {
                throw new ArgumentNullException(nameof(authenticationProvider));
            }

            if (string.IsNullOrWhiteSpace(authenticationProvider.Name))
            {
                throw new ArgumentNullException(nameof(authenticationProvider.Name));
            }

            if (string.IsNullOrWhiteSpace(authenticationProvider.CallbackPath))
            {
                throw new ArgumentNullException(nameof(authenticationProvider.CallbackPath));
            }

            var authProvider = await _authenticationProviderRepository.GetAuthenticationProvider(authenticationProvider.Name);
            if (authProvider != null)
            {
                throw new IdentityConfigurationException(
                    ErrorCodes.InternalErrorCode,
                    string.Format(ErrorDescriptions.TheAuthenticationProviderAlreadyExists, authenticationProvider.Name));
            }

            if (!await _authenticationProviderRepository.AddAuthenticationProvider(authenticationProvider))
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            _configurationEventSource.FinishToAddAuthenticationProvider(json);
            return new NoContentResult();
        }

        #endregion
    }
}
