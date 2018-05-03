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

using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Authorization.Actions
{
    public interface IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation
    {
        Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, IPrincipal claimsPrincipal, Client client);
    }

    public sealed class GetAuthorizationCodeAndTokenViaHybridWorkflowOperation : IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation
    {
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        private readonly IProcessAuthorizationRequest _processAuthorizationRequest;
        private readonly IClientValidator _clientValidator;
        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;

        public GetAuthorizationCodeAndTokenViaHybridWorkflowOperation(
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IProcessAuthorizationRequest processAuthorizationRequest,
            IClientValidator clientValidator,
            IGenerateAuthorizationResponse generateAuthorizationResponse)
        {
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _processAuthorizationRequest = processAuthorizationRequest;
            _clientValidator = clientValidator;
            _generateAuthorizationResponse = generateAuthorizationResponse;
        }

        public async Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, IPrincipal principal, Client client)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(authorizationParameter.Nonce))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.StandardAuthorizationRequestParameterNames.NonceName),
                    authorizationParameter.State);
            }

            var claimsPrincipal = principal == null ? null : principal as ClaimsPrincipal;

            _simpleIdentityServerEventSource.StartHybridFlow(
                authorizationParameter.ClientId,
                authorizationParameter.Scope,
                authorizationParameter.Claims == null ? string.Empty : authorizationParameter.Claims.ToString());
            var result = await _processAuthorizationRequest.ProcessAsync(authorizationParameter, claimsPrincipal, client);
            if (!_clientValidator.CheckGrantTypes(client, GrantType.@implicit, GrantType.authorization_code))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        "implicit"),
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

                await _generateAuthorizationResponse.ExecuteAsync(result, authorizationParameter, claimsPrincipal, client);
            }

            var actionTypeName = Enum.GetName(typeof(TypeActionResult), result.Type);
            _simpleIdentityServerEventSource.EndHybridFlow(
                authorizationParameter.ClientId,
                actionTypeName,
                result.RedirectInstruction == null ? string.Empty : Enum.GetName(typeof(IdentityServerEndPoints), result.RedirectInstruction.Action));

            return result;
        }
    }
}
