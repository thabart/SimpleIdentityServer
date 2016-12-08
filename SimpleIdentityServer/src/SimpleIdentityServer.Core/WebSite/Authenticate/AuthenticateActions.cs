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

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.Authenticate
{
    public interface IAuthenticateActions
    {
        ActionResult AuthenticateResourceOwnerOpenId(
            AuthorizationParameter parameter,
            ClaimsPrincipal claimsPrincipal,
            string code);
        Task<ResourceOwner> LocalUserAuthentication(LocalAuthenticationParameter localAuthenticationParameter);
        Task<LocalOpenIdAuthenticationResult> LocalOpenIdUserAuthentication(
            LocalAuthenticationParameter localAuthenticationParameter,
            AuthorizationParameter authorizationParameter,
            string code);
        Task<ExternalOpenIdAuthenticationResult> ExternalOpenIdUserAuthentication(
            List<Claim> claims,
            AuthorizationParameter authorizationParameter,
            string code);
        Task<IEnumerable<Claim>> LoginCallback(ClaimsPrincipal claimsPrincipal);
        Task<string> GenerateAndSendCode(string subject);
        Task<bool> ValidateCode(string code);
        Task<bool> RemoveCode(string code);
    }

    public class AuthenticateActions : IAuthenticateActions
    {
        private readonly IAuthenticateResourceOwnerOpenIdAction _authenticateResourceOwnerOpenIdAction;
        private readonly ILocalOpenIdUserAuthenticationAction _localOpenIdUserAuthenticationAction;
        private readonly IExternalOpenIdUserAuthenticationAction _externalOpenIdUserAuthenticationAction;
        private readonly ILocalUserAuthenticationAction _localUserAuthenticationAction;
        private readonly ILoginCallbackAction _loginCallbackAction;
        private readonly IGenerateAndSendCodeAction _generateAndSendCodeAction;
        private readonly IValidateConfirmationCodeAction _validateConfirmationCodeAction;
        private readonly IRemoveConfirmationCodeAction _removeConfirmationCodeAction;

        public AuthenticateActions(
            IAuthenticateResourceOwnerOpenIdAction authenticateResourceOwnerOpenIdAction,
            ILocalOpenIdUserAuthenticationAction localOpenIdUserAuthenticationAction,
            IExternalOpenIdUserAuthenticationAction externalOpenIdUserAuthenticationAction,
            ILocalUserAuthenticationAction localUserAuthenticationAction,
            ILoginCallbackAction loginCallbackAction,
            IGenerateAndSendCodeAction generateAndSendCodeAction,
            IValidateConfirmationCodeAction validateConfirmationCodeAction,
            IRemoveConfirmationCodeAction removeConfirmationCodeAction)
        {
            _authenticateResourceOwnerOpenIdAction = authenticateResourceOwnerOpenIdAction;
            _localOpenIdUserAuthenticationAction = localOpenIdUserAuthenticationAction;
            _externalOpenIdUserAuthenticationAction = externalOpenIdUserAuthenticationAction;
            _localUserAuthenticationAction = localUserAuthenticationAction;
            _loginCallbackAction = loginCallbackAction;
            _generateAndSendCodeAction = generateAndSendCodeAction;
            _validateConfirmationCodeAction = validateConfirmationCodeAction;
            _removeConfirmationCodeAction = removeConfirmationCodeAction;
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

        public async Task<ResourceOwner> LocalUserAuthentication(LocalAuthenticationParameter localAuthenticationParameter)
        {
            if (localAuthenticationParameter == null)
            {
                throw new ArgumentNullException("localAuthenticationParameter");
            }

            return await _localUserAuthenticationAction.Execute(localAuthenticationParameter);
        }

        public async Task<LocalOpenIdAuthenticationResult> LocalOpenIdUserAuthentication(
            LocalAuthenticationParameter localAuthenticationParameter,
            AuthorizationParameter authorizationParameter,
            string code)
        {
            if (localAuthenticationParameter == null)
            {
                throw new ArgumentNullException("localAuthenticationParameter");
            }

            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");
            }

            return await _localOpenIdUserAuthenticationAction.Execute(
                localAuthenticationParameter,
                authorizationParameter,
                code);
        }


        public async Task<ExternalOpenIdAuthenticationResult> ExternalOpenIdUserAuthentication(
            List<Claim> claims, 
            AuthorizationParameter authorizationParameter, 
            string code)
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

            return await _externalOpenIdUserAuthenticationAction.Execute(claims,
                authorizationParameter,
                code);
        }

        public async Task<IEnumerable<Claim>> LoginCallback(ClaimsPrincipal claimsPrincipal)
        {
            return await _loginCallbackAction.Execute(claimsPrincipal);
        }

        public async Task<string> GenerateAndSendCode(string subject)
        {
            return await _generateAndSendCodeAction.ExecuteAsync(subject);
        }

        public async Task<bool> ValidateCode(string code)
        {
            return await _validateConfirmationCodeAction.Execute(code);
        }

        public async Task<bool> RemoveCode(string code)
        {
            return await _removeConfirmationCodeAction.Execute(code);
        }
    }
}
