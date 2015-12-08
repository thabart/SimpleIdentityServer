using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Configuration;

namespace SimpleIdentityServer.Core.Api.Authorization.Common
{
    public interface IProcessAuthorizationRequest
    {
        ActionResult Process(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal,
            string code);
    }

    public class ProcessAuthorizationRequest : IProcessAuthorizationRequest
    {
        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IClientValidator _clientValidator;

        private readonly IScopeValidator _scopeValidator;

        private readonly IActionResultFactory _actionResultFactory;

        private readonly IConsentHelper _consentHelper;

        private readonly IJwtParser _jwtParser;

        private readonly ISimpleIdentityServerConfigurator _simpleIdentityServerConfigurator;

        public ProcessAuthorizationRequest(
            IParameterParserHelper parameterParserHelper,
            IClientValidator clientValidator,
            IScopeValidator scopeValidator,
            IActionResultFactory actionResultFactory,
            IConsentHelper consentHelper,
            IJwtParser jwtParser,
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator)
        {
            _parameterParserHelper = parameterParserHelper;
            _clientValidator = clientValidator;
            _scopeValidator = scopeValidator;
            _actionResultFactory = actionResultFactory;
            _consentHelper = consentHelper;
            _jwtParser = jwtParser;
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
        }

        public ActionResult Process(
            AuthorizationParameter authorizationParameter, 
            ClaimsPrincipal claimsPrincipal, 
            string code)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorization parameter may not be null");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException("code parameter may not be null");
            }

            ActionResult result = null;
            var endUserIsAuthenticated = IsAuthenticated(claimsPrincipal);
            var prompts = _parameterParserHelper.ParsePromptParameters(authorizationParameter.Prompt);
            if (prompts == null || !prompts.Any())
            {
                prompts = new List<PromptParameter>();
                if (!endUserIsAuthenticated)
                {
                    prompts.Add(PromptParameter.login);
                }
                else
                {
                    var confirmedConsent = GetResourceOwnerConsent(
                        claimsPrincipal,
                        authorizationParameter);
                    if (confirmedConsent == null)
                    {
                        prompts.Add(PromptParameter.consent);
                    }
                    else
                    {
                        prompts.Add(PromptParameter.none);
                    }
                }
            }

            var client = _clientValidator.ValidateClientExist(authorizationParameter.ClientId);
            if (client == null)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.ClientIsNotValid, authorizationParameter.ClientId),
                    authorizationParameter.State);
            }

            var redirectionUrl = _clientValidator.ValidateRedirectionUrl(authorizationParameter.RedirectUrl, client);
            if (string.IsNullOrWhiteSpace(redirectionUrl))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.RedirectUrlIsNotValid, authorizationParameter.RedirectUrl),
                    authorizationParameter.State);
            }

            string messageError;
            var allowedScopes = _scopeValidator.IsScopesValid(authorizationParameter.Scope, client, out messageError);
            if (!allowedScopes.Any())
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidScope,
                    messageError,
                    authorizationParameter.State);
            }

            if (!allowedScopes.Contains(Constants.StandardScopes.OpenId.Name))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidScope,
                    string.Format(ErrorDescriptions.TheScopesNeedToBeSpecified, Constants.StandardScopes.OpenId.Name),
                    authorizationParameter.State);
            }

            var responseTypes = _parameterParserHelper.ParseResponseType(authorizationParameter.ResponseType);
            if (!responseTypes.Any())
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.StandardAuthorizationRequestParameterNames.ResponseTypeName),
                    authorizationParameter.State);
            }

            if (!_clientValidator.ValidateResponseTypes(responseTypes, client))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType,
                        authorizationParameter.ClientId,
                        string.Join(",", responseTypes)),
                    authorizationParameter.State);
            }

            // Check if the user connection is still valid.
            if (endUserIsAuthenticated &&
                !authorizationParameter.MaxAge.Equals(default(double)))
            {
                var authenticationDateTimeClaim =
                    claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.AuthenticationInstant);
                if (authenticationDateTimeClaim != null)
                {
                    var maxAge = authorizationParameter.MaxAge;
                    var currentDateTimeUtc = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
                    var authenticationDateTime = long.Parse(authenticationDateTimeClaim.Value);
                    if (maxAge < currentDateTimeUtc - authenticationDateTime)
                    {
                        result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                        result.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.AuthorizationCodeName, code);
                        result.RedirectInstruction.Action = IdentityServerEndPoints.AuthenticateIndex;
                        return result;
                    }
                }
            }

            result = ProcessPromptParameters(
                prompts,
                claimsPrincipal,
                authorizationParameter,
                code);

            ProcessIdTokenHint(result,
                authorizationParameter,
                prompts);

            return result;
        }

        private void ProcessIdTokenHint(
            ActionResult actionResult,
            AuthorizationParameter authorizationParameter,
            List<PromptParameter> prompts)
        {
            if (!string.IsNullOrWhiteSpace(authorizationParameter.IdTokenHint) &&
                prompts.Contains(PromptParameter.none) &&
                actionResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var jwsToken = _jwtParser.Decrypt(authorizationParameter.IdTokenHint);
                if (jwsToken == null)
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheIdTokenHintParameterCannotBeDecrypted,
                        authorizationParameter.State);
                }

                var jwsPayload = _jwtParser.UnSign(jwsToken);
                if (jwsPayload == null)
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheSignatureOfIdTokenHintParameterCannotBeChecked,
                        authorizationParameter.State);
                }

                var issuerName = _simpleIdentityServerConfigurator.GetIssuerName();
                if (jwsPayload.Audiences == null ||
                    !jwsPayload.Audiences.Any() ||
                    jwsPayload.Audiences.Contains(issuerName))
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheIdentityTokenDoesntContainSimpleIdentityServerAsAudience,
                        authorizationParameter.State);
                }
            }
        }


        /// <summary>
        /// Process the prompt authorizationParameter.
        /// </summary>
        /// <param name="prompts">Prompt authorizationParameter values</param>
        /// <param name="principal">User's claims</param>
        /// <param name="authorizationParameter">Authorization code grant-type authorizationParameter</param>
        /// <param name="code">Encrypted and signed authorization code grant-type authorizationParameter</param>
        /// <returns>The action result interpreted by the controller</returns>
        private ActionResult ProcessPromptParameters(
            IList<PromptParameter> prompts,
            ClaimsPrincipal principal,
            AuthorizationParameter authorizationParameter,
            string code)
        {
            var endUserIsAuthenticated = IsAuthenticated(principal);

            // Raise "login_required" exception : if the prompt authorizationParameter is "none" AND the user is not authenticated
            // Raise "interaction_required" exception : if there's no consent from the user.
            if (prompts.Contains(PromptParameter.none))
            {
                if (!endUserIsAuthenticated)
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.LoginRequiredCode,
                        ErrorDescriptions.TheUserNeedsToBeAuthenticated,
                        authorizationParameter.State);
                }

                var confirmedConsent = GetResourceOwnerConsent(principal, authorizationParameter);
                if (confirmedConsent == null)
                {
                    throw new IdentityServerExceptionWithState(
                            ErrorCodes.InteractionRequiredCode,
                            ErrorDescriptions.TheUserNeedsToGiveHisConsent,
                            authorizationParameter.State);
                }

                var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                return result;
            }

            // Redirects to the authentication screen 
            // if the "prompt" authorizationParameter is equal to "login" OR
            // The user is not authenticated AND the prompt authorizationParameter is different from "none"
            if (prompts.Contains(PromptParameter.login))
            {
                var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                result.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.AuthorizationCodeName, code);
                result.RedirectInstruction.Action = IdentityServerEndPoints.AuthenticateIndex;
                return result;
            }

            if (prompts.Contains(PromptParameter.consent))
            {
                var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                result.RedirectInstruction.AddParameter(Constants.StandardAuthorizationResponseNames.AuthorizationCodeName, code);
                if (!endUserIsAuthenticated)
                {
                    result.RedirectInstruction.Action = IdentityServerEndPoints.AuthenticateIndex;
                    return result;
                }

                result.RedirectInstruction.Action = IdentityServerEndPoints.ConsentIndex;
                return result;
            }

            return null;
        }

        private Consent GetResourceOwnerConsent(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            var subject = claimsPrincipal.GetSubject();
            return _consentHelper.GetConsentConfirmedByResourceOwner(subject, authorizationParameter);
        }

        private static bool IsAuthenticated(ClaimsPrincipal principal)
        {
            return principal == null || principal.Identity == null ?
                false :
                principal.Identity.IsAuthenticated;
        }
    }
}
