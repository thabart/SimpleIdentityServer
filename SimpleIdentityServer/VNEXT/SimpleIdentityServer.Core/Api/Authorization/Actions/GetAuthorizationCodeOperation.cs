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
using System.Security.Claims;
using System.Security.Principal;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Core.Api.Authorization.Actions
{
    public interface IGetAuthorizationCodeOperation
    {
        ActionResult Execute(
            AuthorizationParameter authorizationParameter, 
            IPrincipal claimsPrincipal);
    }

    public class GetAuthorizationCodeOperation : IGetAuthorizationCodeOperation
    {
        private readonly IProcessAuthorizationRequest _processAuthorizationRequest;

        private IClientValidator _clientValidator;

        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;

        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        public GetAuthorizationCodeOperation(
            IProcessAuthorizationRequest processAuthorizationRequest,
            IClientValidator clientValidator,
            IGenerateAuthorizationResponse generateAuthorizationResponse,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource)
        {
            _processAuthorizationRequest = processAuthorizationRequest;
            _clientValidator = clientValidator;
            _generateAuthorizationResponse = generateAuthorizationResponse;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
        }

        public ActionResult Execute(
            AuthorizationParameter authorizationParameter,
            IPrincipal principal)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");
            }

            var claimsPrincipal = principal == null ? null : principal as ClaimsPrincipal;

            _simpleIdentityServerEventSource.StartAuthorizationCodeFlow(
                authorizationParameter.ClientId,
                authorizationParameter.Scope,
                authorizationParameter.Claims == null ? string.Empty : authorizationParameter.Claims.ToString());
            var result = _processAuthorizationRequest.Process(authorizationParameter,
                claimsPrincipal);
            var client = _clientValidator.ValidateClientExist(authorizationParameter.ClientId);
            if (!_clientValidator.ValidateGrantType(GrantType.authorization_code, client))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        "authorization_code"),
                    authorizationParameter.State);
            }

            if (result.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                if (claimsPrincipal == null)
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheResponseCannotBeGeneratedBecauseResourceOwnerNeedsToBeAuthenticated,
                        authorizationParameter.State);
                }

                _generateAuthorizationResponse.Execute(result, authorizationParameter,
                    claimsPrincipal);
            }

            var actionTypeName = Enum.GetName(typeof(TypeActionResult), result.Type);
            _simpleIdentityServerEventSource.EndAuthorizationCodeFlow(
                authorizationParameter.ClientId,
                actionTypeName,
                result.RedirectInstruction == null ? string.Empty : Enum.GetName(typeof(IdentityServerEndPoints), result.RedirectInstruction.Action));

            return result;
        }
    }
}
