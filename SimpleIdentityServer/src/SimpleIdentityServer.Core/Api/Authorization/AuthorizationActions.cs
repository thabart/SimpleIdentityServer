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

using SimpleIdentityServer.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Core.Bus;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Events;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Authorization
{
    public interface IAuthorizationActions
    {
        Task<ActionResult> GetAuthorization(AuthorizationParameter parameter, IPrincipal claimsPrincipal);
    }

    public class AuthorizationActions : IAuthorizationActions
    {
        private readonly IGetAuthorizationCodeOperation _getAuthorizationCodeOperation;
        private readonly IGetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation;
        private readonly IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation;
        private readonly IAuthorizationCodeGrantTypeParameterAuthEdpValidator _authorizationCodeGrantTypeParameterValidator;
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        private readonly IAuthorizationFlowHelper _authorizationFlowHelper;
        private readonly IEventPublisher _eventPublisher;
        
        public AuthorizationActions(
            IGetAuthorizationCodeOperation getAuthorizationCodeOperation,
            IGetTokenViaImplicitWorkflowOperation getTokenViaImplicitWorkflowOperation,
            IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation getAuthorizationCodeAndTokenViaHybridWorkflowOperation,
            IAuthorizationCodeGrantTypeParameterAuthEdpValidator authorizationCodeGrantTypeParameterValidator,
            IParameterParserHelper parameterParserHelper,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IAuthorizationFlowHelper authorizationFlowHelper,
            IEventPublisher eventPublisher)
        {
            _getAuthorizationCodeOperation = getAuthorizationCodeOperation;
            _getTokenViaImplicitWorkflowOperation = getTokenViaImplicitWorkflowOperation;
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation =
                getAuthorizationCodeAndTokenViaHybridWorkflowOperation;
            _authorizationCodeGrantTypeParameterValidator = authorizationCodeGrantTypeParameterValidator;
            _parameterParserHelper = parameterParserHelper;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _authorizationFlowHelper = authorizationFlowHelper;
            _eventPublisher = eventPublisher;
        }

        public async Task<ActionResult> GetAuthorization(AuthorizationParameter parameter, IPrincipal claimsPrincipal)
        {
            var processId = Guid.NewGuid().ToString();
            _eventPublisher.Publish(new AuthorizationRequestReceived(Guid.NewGuid().ToString(), processId,  parameter, 0));
            try
            {
                var client = await _authorizationCodeGrantTypeParameterValidator.ValidateAsync(parameter).ConfigureAwait(false);
                ActionResult actionResult = null;
                _simpleIdentityServerEventSource.StartAuthorization(parameter.ClientId,
                    parameter.ResponseType,
                    parameter.Scope,
                    parameter.Claims == null ? string.Empty : parameter.Claims.ToString());
                if (client.RequirePkce && (string.IsNullOrWhiteSpace(parameter.CodeChallenge) || parameter.CodeChallengeMethod == null))
                {
                    throw new IdentityServerException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheClientRequiresPkce, parameter.ClientId));
                }

                var responseTypes = _parameterParserHelper.ParseResponseTypes(parameter.ResponseType);
                var authorizationFlow = _authorizationFlowHelper.GetAuthorizationFlow(responseTypes, parameter.State);
                switch (authorizationFlow)
                {
                    case AuthorizationFlow.AuthorizationCodeFlow:
                        actionResult = await _getAuthorizationCodeOperation.Execute(parameter, claimsPrincipal, client).ConfigureAwait(false);
                        break;
                    case AuthorizationFlow.ImplicitFlow:
                        actionResult = await _getTokenViaImplicitWorkflowOperation.Execute(parameter, claimsPrincipal, client).ConfigureAwait(false);
                        break;
                    case AuthorizationFlow.HybridFlow:
                        actionResult = await _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(parameter, claimsPrincipal, client).ConfigureAwait(false);
                        break;
                }

                if (actionResult != null)
                {
                    var actionTypeName = Enum.GetName(typeof(TypeActionResult), actionResult.Type);
                    var actionName = string.Empty;
                    if (actionResult.Type == TypeActionResult.RedirectToAction)
                    {
                        var actionEnum = actionResult.RedirectInstruction.Action;
                        actionName = Enum.GetName(typeof(IdentityServerEndPoints), actionEnum);
                    }

                    var serializedParameters = actionResult.RedirectInstruction == null || actionResult.RedirectInstruction.Parameters == null ? String.Empty :
                        actionResult.RedirectInstruction.Parameters.SerializeWithJavascript();
                    _simpleIdentityServerEventSource.EndAuthorization(actionTypeName,
                        actionName,
                        serializedParameters);
                }

                _eventPublisher.Publish(new AuthorizationGranted(Guid.NewGuid().ToString(), processId, actionResult, 1));
                actionResult.ProcessId = processId;
                return actionResult;
            }
            catch(IdentityServerException ex)
            {
                _eventPublisher.Publish(new OpenIdErrorReceived(Guid.NewGuid().ToString(), processId, ex.Code, ex.Message, 1));
                throw;
            }
        }
    }
}
