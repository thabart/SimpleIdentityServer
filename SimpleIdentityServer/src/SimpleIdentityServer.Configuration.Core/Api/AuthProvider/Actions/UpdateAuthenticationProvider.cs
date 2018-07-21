﻿#region copyright
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
using SimpleIdentityServer.Configuration.Core.Models;
using SimpleIdentityServer.Configuration.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Core.Api.AuthProvider.Actions
{
    public interface IUpdateAuthenticationProvider
    {
        Task<ActionResult> ExecuteAsync(AuthenticationProvider authenticationProvider);
    }

    internal class UpdateAuthenticationProvider : IUpdateAuthenticationProvider
    {
        private readonly IAuthenticationProviderRepository _authenticationProviderRepository;

        public UpdateAuthenticationProvider(IAuthenticationProviderRepository authenticationProviderRepository)
        {
            _authenticationProviderRepository = authenticationProviderRepository;
        }

        public async Task<ActionResult> ExecuteAsync(AuthenticationProvider authenticationProvider)
        {
            if (authenticationProvider == null)
            {
                throw new ArgumentNullException(nameof(authenticationProvider));
            }

            var authProvider = await _authenticationProviderRepository.GetAuthenticationProvider(authenticationProvider.Name).ConfigureAwait(false);
            if (authProvider == null)
            {
                return new NotFoundResult();
            }
            
            var isUpdated = await _authenticationProviderRepository.UpdateAuthenticationProvider(authenticationProvider).ConfigureAwait(false);
            if (isUpdated)
            {
                return new NoContentResult();
            }

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
