using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using SimpleIdentityServer.Core.Api.Authorization.Actions;
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

                _simpleIdentityServerEventSource.EndAuthorization(actionTypeName, actionName);
            }

            return actionResult;
        }

        private static AuthorizationFlow GetAuthorizationFlow(ICollection<ResponseType> responseTypes, string state)
        {
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
