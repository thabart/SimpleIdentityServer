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
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Responses;
using SimpleIdentityServer.Uma.Core.Api.Authorization.Actions;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.Authorization
{
    public interface IAuthorizationActions
    {
        Task<AuthorizationResponse> GetAuthorization(GetAuthorizationActionParameter getAuthorizationActionParameter,
            IEnumerable<Claim> claims);
    }

    internal class AuthorizationActions : IAuthorizationActions
    {
        private readonly IGetAuthorizationAction _getAuthorizationAction;

        #region Constructor

        public AuthorizationActions(IGetAuthorizationAction getAuthorizationAction)
        {
            _getAuthorizationAction = getAuthorizationAction;
        }

        #endregion

        #region Public methods

        public async Task<AuthorizationResponse> GetAuthorization(GetAuthorizationActionParameter getAuthorizationActionParameter,
            IEnumerable<Claim> claims)
        {
            return await _getAuthorizationAction.Execute(getAuthorizationActionParameter,
                claims);
        }

        #endregion
    }
}
