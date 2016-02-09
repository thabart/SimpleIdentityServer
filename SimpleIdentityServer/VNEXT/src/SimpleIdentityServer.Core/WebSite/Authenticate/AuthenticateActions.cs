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
using System.Linq;
using System.Security.Claims;

using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;

using System;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Errors;

namespace SimpleIdentityServer.Core.WebSite.Authenticate
{
    public interface IAuthenticateActions
    {
        ActionResult AuthenticateResourceOwnerOpenId(
            AuthorizationParameter parameter,
            ClaimsPrincipal claimsPrincipal,
            string code);

        List<Claim> LocalUserAuthentication(LocalAuthenticationParameter localAuthenticationParameter);

        List<Claim> ExternalUserAuthentication(List<Claim> claims, string providerType);

        ActionResult LocalOpenIdUserAuthentication(
            LocalAuthenticationParameter localAuthenticationParameter,
            AuthorizationParameter authorizationParameter,
            string code,
            out List<Claim> claims);

        ActionResult ExternalOpenIdUserAuthentication(
            List<Claim> claims,
            AuthorizationParameter authorizationParameter,
            string code,
            string providerType);
    }

    public class AuthenticateActions : IAuthenticateActions
    {
        private readonly IAuthenticateResourceOwnerOpenIdAction _authenticateResourceOwnerOpenIdAction;

        private readonly ILocalOpenIdUserAuthenticationAction _localOpenIdUserAuthenticationAction;

        private readonly IExternalOpenIdUserAuthenticationAction _externalOpenIdUserAuthenticationAction;

        private readonly ILocalUserAuthenticationAction _localUserAuthenticationAction;

        private readonly IExternalUserAuthenticationAction _externalUserAuthenticationAction;

        public AuthenticateActions(
            IAuthenticateResourceOwnerOpenIdAction authenticateResourceOwnerOpenIdAction,
            ILocalOpenIdUserAuthenticationAction localOpenIdUserAuthenticationAction,
            IExternalOpenIdUserAuthenticationAction externalOpenIdUserAuthenticationAction,
            ILocalUserAuthenticationAction localUserAuthenticationAction,
            IExternalUserAuthenticationAction externalUserAuthenticationAction)
        {
            _authenticateResourceOwnerOpenIdAction = authenticateResourceOwnerOpenIdAction;
            _localOpenIdUserAuthenticationAction = localOpenIdUserAuthenticationAction;
            _externalOpenIdUserAuthenticationAction = externalOpenIdUserAuthenticationAction;
            _localUserAuthenticationAction = localUserAuthenticationAction;
            _externalUserAuthenticationAction = externalUserAuthenticationAction;
        }

        public ActionResult AuthenticateResourceOwnerOpenId(
            AuthorizationParameter parameter,
            ClaimsPrincipal claimsPrincipal,
            string code)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException("claimsPrincipal");
            }

            return _authenticateResourceOwnerOpenIdAction.Execute(parameter, 
                claimsPrincipal, 
                code);
        }

        public List<Claim> LocalUserAuthentication(LocalAuthenticationParameter localAuthenticationParameter)
        {
            if (localAuthenticationParameter == null)
            {
                throw new ArgumentNullException("localAuthenticationParameter");
            }

            return _localUserAuthenticationAction.Execute(localAuthenticationParameter);
        }

        public List<Claim> ExternalUserAuthentication(List<Claim> claims, string providerType)
        {
            if (claims == null || !claims.Any())
            {
                throw new ArgumentNullException("claims");
            }
            
            if (string.IsNullOrWhiteSpace(providerType))
            {
                throw new ArgumentNullException("providerType");
            }

            return _externalUserAuthenticationAction.Execute(claims, providerType);
        }

        public ActionResult LocalOpenIdUserAuthentication(
            LocalAuthenticationParameter localAuthenticationParameter,
            AuthorizationParameter authorizationParameter,
            string code,
            out List<Claim> claims)
        {
            if (localAuthenticationParameter == null)
            {
                throw new ArgumentNullException("localAuthenticationParameter");
            }

            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");
            }

            return _localOpenIdUserAuthenticationAction.Execute(
                localAuthenticationParameter,
                authorizationParameter,
                code,
                out claims);
        }


        public ActionResult ExternalOpenIdUserAuthentication(
            List<Claim> claims, 
            AuthorizationParameter authorizationParameter, 
            string code,
            string providerType)
        {
            if (claims == null || !claims.Any())
            {
                throw new ArgumentNullException("claims");
            }

            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException("code");
            }

            if (string.IsNullOrWhiteSpace(providerType))
            {
                throw new ArgumentNullException("providerType");
            }

            if (!Constants.SupportedProviderTypes.Contains(providerType))
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode,
                    string.Format(ErrorDescriptions.TheExternalProviderIsNotSupported, providerType));
            }

            return _externalOpenIdUserAuthenticationAction.Execute(claims,
                authorizationParameter,
                code,
                providerType);
        }
    }
}
