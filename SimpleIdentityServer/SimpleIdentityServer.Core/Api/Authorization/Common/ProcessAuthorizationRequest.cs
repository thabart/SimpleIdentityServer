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
using System.Security.Claims;
using SimpleIdentityServer.Core.Common.Extensions;
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
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Core.Api.Authorization.Common
{
    public interface IProcessAuthorizationRequest
    {
        ActionResult Process(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal);
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

        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        public ProcessAuthorizationRequest(
            IParameterParserHelper parameterParserHelper,
            IClientValidator clientValidator,
            IScopeValidator scopeValidator,
            IActionResultFactory actionResultFactory,
            IConsentHelper consentHelper,
            IJwtParser jwtParser,
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource)
        {
            _parameterParserHelper = parameterParserHelper;
            _clientValidator = clientValidator;
            _scopeValidator = scopeValidator;
            _actionResultFactory = actionResultFactory;
            _consentHelper = consentHelper;
            _jwtParser = jwtParser;
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
        }

        public ActionResult Process(
            AuthorizationParameter authorizationParameter, 
            ClaimsPrincipal claimsPrincipal)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException("authorizationParameter");
            }

            var serializedAuthorizationParameter = authorizationParameter.SerializeWithJavascript();
            _simpleIdentityServerEventSource.StartProcessingAuthorizationRequest(serializedAuthorizationParameter);

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
                        result.RedirectInstruction.Action = IdentityServerEndPoints.AuthenticateIndex;
                    }
                }
            }

            if (result == null)
            {
                result = ProcessPromptParameters(
                    prompts,
                    claimsPrincipal,
                    authorizationParameter);

                ProcessIdTokenHint(result,
                    authorizationParameter,
                    prompts,
                    claimsPrincipal);
            }

            var actionTypeName = Enum.GetName(typeof(TypeActionResult), result.Type);
            var actionName = result.RedirectInstruction == null 
                ? string.Empty 
                : Enum.GetName(typeof(IdentityServerEndPoints), result.RedirectInstruction.Action);
            _simpleIdentityServerEventSource.EndProcessingAuthorizationRequest(
                serializedAuthorizationParameter,
                actionTypeName,
                actionName);

            return result;
        }

        private void ProcessIdTokenHint(
            ActionResult actionResult,
            AuthorizationParameter authorizationParameter,
            ICollection<PromptParameter> prompts,
            ClaimsPrincipal claimsPrincipal)
        {
            if (!string.IsNullOrWhiteSpace(authorizationParameter.IdTokenHint) &&
                prompts.Contains(PromptParameter.none) &&
                actionResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var token = authorizationParameter.IdTokenHint;
                if (!_jwtParser.IsJweToken(token) &&
                    !_jwtParser.IsJwsToken(token))
                {
                    throw new IdentityServerExceptionWithState(
                            ErrorCodes.InvalidRequestCode,
                            ErrorDescriptions.TheIdTokenHintParameterIsNotAValidToken,
                            authorizationParameter.State);
                }

                string jwsToken;
                if (_jwtParser.IsJweToken(token))
                {
                    jwsToken = _jwtParser.Decrypt(token);
                    if (string.IsNullOrWhiteSpace(jwsToken))
                    {
                        throw new IdentityServerExceptionWithState(
                            ErrorCodes.InvalidRequestCode,
                            ErrorDescriptions.TheIdTokenHintParameterCannotBeDecrypted,
                            authorizationParameter.State);
                    }
                }
                else
                {
                    jwsToken = token;
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
                    !jwsPayload.Audiences.Contains(issuerName))
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheIdentityTokenDoesntContainSimpleIdentityServerAsAudience,
                        authorizationParameter.State);
                }

                var currentSubject = string.Empty;
                var expectedSubject = jwsPayload.GetClaimValue(Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
                if (claimsPrincipal != null && claimsPrincipal.IsAuthenticated())
                {
                    currentSubject = claimsPrincipal.GetSubject();
                }

                if (currentSubject != expectedSubject)
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken,
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
        /// <returns>The action result interpreted by the controller</returns>
        private ActionResult ProcessPromptParameters(
            ICollection<PromptParameter> prompts,
            ClaimsPrincipal principal,
            AuthorizationParameter authorizationParameter)
        {
            if (prompts == null || !prompts.Any())
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationRequestCannotBeProcessedBecauseThereIsNotValidPrompt,
                    authorizationParameter.State);
            }

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
                result.RedirectInstruction.Action = IdentityServerEndPoints.AuthenticateIndex;
                return result;
            }

            if (prompts.Contains(PromptParameter.consent))
            {
                var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                if (!endUserIsAuthenticated)
                {
                    result.RedirectInstruction.Action = IdentityServerEndPoints.AuthenticateIndex;
                    return result;
                }

                result.RedirectInstruction.Action = IdentityServerEndPoints.ConsentIndex;
                return result;
            }

            throw new IdentityServerExceptionWithState(
                ErrorCodes.InvalidRequestCode,
                string.Format(ErrorDescriptions.ThePromptParameterIsNotSupported, string.Join(",", prompts)),
                authorizationParameter.State);
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
