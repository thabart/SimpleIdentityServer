using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Common
{
    public interface IAuthenticateHelper
    {
        ActionResult ProcessRedirection(
            AuthorizationParameter authorizationParameter,
            string code,
            string subject,
            List<Claim> claims);
    }

    public sealed class AuthenticateHelper : IAuthenticateHelper
    {
        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IActionResultFactory _actionResultFactory;

        private readonly IConsentHelper _consentHelper;

        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;

        public AuthenticateHelper(IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory,
            IConsentHelper consentHelper,
            IGenerateAuthorizationResponse generateAuthorizationResponse)
        {
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
            _consentHelper = consentHelper;
            _generateAuthorizationResponse = generateAuthorizationResponse;
        }

        public ActionResult ProcessRedirection(
            AuthorizationParameter authorizationParameter,
            string code,
            string subject,
            List<Claim> claims)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");
            }

            // Redirect to the consent page if the prompt parameter contains "consent"
            ActionResult result;
            var prompts = _parameterParserHelper.ParsePromptParameters(authorizationParameter.Prompt);
            if (prompts != null &&
                prompts.Contains(PromptParameter.consent))
            {
                result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                result.RedirectInstruction.Action = IdentityServerEndPoints.ConsentIndex;
                result.RedirectInstruction.AddParameter("code", code);
                return result;
            }

            var assignedConsent = _consentHelper.GetConsentConfirmedByResourceOwner(subject, authorizationParameter);

            // If there's already one consent then redirect to the callback
            if (assignedConsent != null)
            {
                result = _actionResultFactory.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                var claimsIdentity = new ClaimsIdentity(claims, "simpleIdentityServer");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                //// TODO : Pass the client
                _generateAuthorizationResponse.Execute(result, authorizationParameter, claimsPrincipal, null);
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseMode.None)
                {
                    var responseTypes = _parameterParserHelper.ParseResponseType(authorizationParameter.ResponseType);
                    var authorizationFlow = GetAuthorizationFlow(responseTypes, authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                result.RedirectInstruction.ResponseMode = responseMode;
                return result;
            }

            // If there's no consent & there's no consent prompt then redirect to the consent screen.
            result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
            result.RedirectInstruction.Action = IdentityServerEndPoints.ConsentIndex;
            result.RedirectInstruction.AddParameter("code", code);
            return result;
        }

        #region Private static methods

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

        private static ResponseMode GetResponseMode(AuthorizationFlow authorizationFlow)
        {
            return Constants.MappingAuthorizationFlowAndResponseModes[authorizationFlow];
        }

        #endregion
    }
}
