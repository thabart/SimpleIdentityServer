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

using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Services;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface ILocalUserAuthenticationAction
    {
        Task<ResourceOwner> Execute(LocalAuthenticationParameter localAuthenticationParameter);
    }

    public sealed class LocalUserAuthenticationAction : ILocalUserAuthenticationAction
    {
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

        public LocalUserAuthenticationAction(IAuthenticateResourceOwnerService authenticateResourceOwnerService)
        {
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
        }
        
        public async Task<ResourceOwner> Execute(LocalAuthenticationParameter localAuthenticationParameter)
        {
            if (localAuthenticationParameter == null)
            {
                throw new ArgumentNullException("localAuthenticationParameter");
            }

            var record = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(localAuthenticationParameter.UserName,
                localAuthenticationParameter.Password);
            if (record == null)
            {
                throw new IdentityServerAuthenticationException(ErrorDescriptions.TheResourceOwnerCredentialsAreNotCorrect);
            }

            return record;
        }
    }
}
