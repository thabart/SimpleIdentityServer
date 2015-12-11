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
using System.Collections.Generic;
using System.Security.Claims;

using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.WebSite.Authenticate
{
    public interface IAuthenticateActions
    {
        ActionResult AuthenticateResourceOwner(
            AuthorizationParameter parameter,
            ClaimsPrincipal claimsPrincipal,
            string code);

        ActionResult LocalUserAuthentication(
            LocalAuthorizationParameter localAuthorizationParameter,
            AuthorizationParameter parameter,
            ClaimsPrincipal claimsPrincipal,
            string code,
            out List<Claim> claims);
    }

    public class AuthenticateActions : IAuthenticateActions
    {
        private readonly IAuthenticateResourceOwnerAction _authenticateResourceOwnerAction;

        private readonly ILocalUserAuthenticationAction _localUserAuthenticationAction;

        public AuthenticateActions(
            IAuthenticateResourceOwnerAction authenticateResourceOwnerAction,
            ILocalUserAuthenticationAction localUserAuthenticationAction)
        {
            _authenticateResourceOwnerAction = authenticateResourceOwnerAction;
            _localUserAuthenticationAction = localUserAuthenticationAction;
        }

        public ActionResult AuthenticateResourceOwner(
            AuthorizationParameter parameter,
            ClaimsPrincipal claimsPrincipal,
            string code)
        {
            return _authenticateResourceOwnerAction.Execute(parameter, claimsPrincipal, code);
        }

        public ActionResult LocalUserAuthentication(
            LocalAuthorizationParameter localAuthorizationParameter,
            AuthorizationParameter parameter,
            ClaimsPrincipal claimsPrincipal,
            string code,
            out List<Claim> claims)
        {
            return _localUserAuthenticationAction.Execute(
                localAuthorizationParameter,
                parameter,
                claimsPrincipal,
                code,
                out claims);
        }
    }
}
