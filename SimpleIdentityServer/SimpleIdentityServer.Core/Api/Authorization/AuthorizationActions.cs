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
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using SimpleIdentityServer.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Core.Api.Authorization
{
    public interface IAuthorizationActions
    {
        ActionResult GetAuthorization(AuthorizationParameter parameter,
            IPrincipal claimsPrincipal,
            string code);
    }

    public class AuthorizationActions : IAuthorizationActions
    {
        private readonly IGetAuthorizationCodeOperation _getAuthorizationCodeOperation;

        private readonly IGetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation;

        private readonly IAuthorizationCodeGrantTypeParameterAuthEdpValidator _authorizationCodeGrantTypeParameterValidator;

        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        
        public AuthorizationActions(
            IGetAuthorizationCodeOperation getAuthorizationCodeOperation,
            IGetTokenViaImplicitWorkflowOperation getTokenViaImplicitWorkflowOperation,
            IAuthorizationCodeGrantTypeParameterAuthEdpValidator authorizationCodeGrantTypeParameterValidator,
            IParameterParserHelper parameterParserHelper,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource)
        {
            _getAuthorizationCodeOperation = getAuthorizationCodeOperation;
            _getTokenViaImplicitWorkflowOperation = getTokenViaImplicitWorkflowOperation;
            _authorizationCodeGrantTypeParameterValidator = authorizationCodeGrantTypeParameterValidator;
            _parameterParserHelper = parameterParserHelper;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
        }

        public ActionResult GetAuthorization(AuthorizationParameter parameter,
            IPrincipal claimsPrincipal,
            string code)
        {
            _authorizationCodeGrantTypeParameterValidator.Validate(parameter);

            ActionResult actionResult = null;
            _simpleIdentityServerEventSource.StartAuthorization(parameter.ClientId,
                parameter.ResponseType,
                parameter.Scope,
                parameter.Claims == null ? string.Empty : parameter.Claims.ToString());
            var responseTypes = _parameterParserHelper.ParseResponseType(parameter.ResponseType);
            var authorizationFlow = GetAuthorizationFlow(responseTypes, parameter.State);
            switch (authorizationFlow)
            {
                case AuthorizationFlow.AuthorizationCodeFlow:
                    actionResult = _getAuthorizationCodeOperation.Execute(
                        parameter,
                        claimsPrincipal,
                        code);
                    break;
                case AuthorizationFlow.ImplicitFlow:
                    actionResult =  _getTokenViaImplicitWorkflowOperation.Execute(
                        parameter,
                        claimsPrincipal,
                        code);
                    break;
                case AuthorizationFlow.HybridFlow:

                    break;
            }

            if (actionResult != null)
            {
                var actionTypeName = Enum.GetName(typeof(TypeActionResult), actionResult.Type);
                var actionName = string.Empty;
                if (actionResult.Type == TypeActionResult.RedirectToAction)
                {
                    var actionEnum = actionResult.RedirectInstruction.Action;
                    actionName = Enum.GetName(typeof (IdentityServerEndPoints), actionEnum);
                }

                var serializedParameters = actionResult.RedirectInstruction == null || actionResult.RedirectInstruction.Parameters == null ? String.Empty :
                    actionResult.RedirectInstruction.Parameters.SerializeWithJavascript();
                _simpleIdentityServerEventSource.EndAuthorization(actionTypeName, 
                    actionName,
                    serializedParameters);
            }

            return actionResult;
        }

        private static AuthorizationFlow GetAuthorizationFlow(ICollection<ResponseType> responseTypes, string state)
        {
            if (responseTypes == null)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            var record = Constants.MappingResponseTypesToAuthorizationFlows.Keys
                .SingleOrDefault(k => k.Count == responseTypes.Count && k.All(key => responseTypes.Contains(key)));
            if (record == null)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            return Constants.MappingResponseTypesToAuthorizationFlows[record];
        }
    }
}
