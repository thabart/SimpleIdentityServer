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

using SimpleIdServer.Bus;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.Authenticate
{
    public interface IAuthenticateActions
    {
        Task<ActionResult> AuthenticateResourceOwnerOpenId(AuthorizationParameter parameter, ClaimsPrincipal claimsPrincipal, string code, string issuerName);
        Task<LocalOpenIdAuthenticationResult> LocalOpenIdUserAuthentication(LocalAuthenticationParameter localAuthenticationParameter, AuthorizationParameter authorizationParameter, string code, string issuerName);
        Task<string> GenerateAndSendCode(string subject);
        Task<bool> ValidateCode(string code);
        Task<bool> RemoveCode(string code);
    }

    public class AuthenticateActions : IAuthenticateActions
    {
        private readonly IAuthenticateResourceOwnerOpenIdAction _authenticateResourceOwnerOpenIdAction;
        private readonly ILocalOpenIdUserAuthenticationAction _localOpenIdUserAuthenticationAction;
        private readonly IGenerateAndSendCodeAction _generateAndSendCodeAction;
        private readonly IValidateConfirmationCodeAction _validateConfirmationCodeAction;
        private readonly IRemoveConfirmationCodeAction _removeConfirmationCodeAction;
        private readonly IEventPublisher _eventPublisher;

        public AuthenticateActions(
            IAuthenticateResourceOwnerOpenIdAction authenticateResourceOwnerOpenIdAction,
            ILocalOpenIdUserAuthenticationAction localOpenIdUserAuthenticationAction,
            IGenerateAndSendCodeAction generateAndSendCodeAction,
            IValidateConfirmationCodeAction validateConfirmationCodeAction,
            IRemoveConfirmationCodeAction removeConfirmationCodeAction)
        {
            _authenticateResourceOwnerOpenIdAction = authenticateResourceOwnerOpenIdAction;
            _localOpenIdUserAuthenticationAction = localOpenIdUserAuthenticationAction;
            _generateAndSendCodeAction = generateAndSendCodeAction;
            _validateConfirmationCodeAction = validateConfirmationCodeAction;
            _removeConfirmationCodeAction = removeConfirmationCodeAction;
        }

        public async Task<LocalOpenIdAuthenticationResult> LocalOpenIdUserAuthentication(LocalAuthenticationParameter localAuthenticationParameter, AuthorizationParameter authorizationParameter, string code, string issuerName)
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
                code, issuerName);
        }

        public async Task<ActionResult> AuthenticateResourceOwnerOpenId(AuthorizationParameter parameter, ClaimsPrincipal claimsPrincipal, string code, string issuerName)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            return await _authenticateResourceOwnerOpenIdAction.Execute(parameter, 
                claimsPrincipal, 
                code, issuerName);
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
