using System;
using System.Collections.Generic;
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
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.Api.Authorization.Actions
{
    public interface IGetAuthorizationCodeOperation
    {
        ActionResult Execute(
            AuthorizationCodeGrantTypeParameter parameter, 
            IPrincipal claimsPrincipal,
            string code);
    }

    public class GetAuthorizationCodeOperation : IGetAuthorizationCodeOperation
    {
        private readonly IScopeValidator _scopeValidator;

        private readonly IClientValidator _clientValidator;

        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IActionResultFactory _actionResultFactory;

        private readonly IConsentRepository _consentRepository;

        private readonly IAuthorizationCodeRepository _authorizationCodeRepository;

        public GetAuthorizationCodeOperation(
            IClientValidator clientValidator,
            IScopeValidator scopeValidator,
            IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory,
            IConsentRepository consentRepository,
            IAuthorizationCodeRepository authorizationCodeRepository)
        {
            _clientValidator = clientValidator;
            _scopeValidator = scopeValidator;
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
            _consentRepository = consentRepository;
            _authorizationCodeRepository = authorizationCodeRepository;
        }

        public ActionResult Execute(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            IPrincipal claimsPrincipal,
            string code)
        {
            authorizationCodeGrantTypeParameter.Validate();
            var client = _clientValidator.ValidateClientExist(authorizationCodeGrantTypeParameter.ClientId);
            _clientValidator.ValidateRedirectionUrl(authorizationCodeGrantTypeParameter.RedirectUrl, client);
            var allowedScopes = _scopeValidator.ValidateAllowedScopes(authorizationCodeGrantTypeParameter.Scope, client);
            if (!allowedScopes.Contains("openid"))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestUriCode,
                    string.Format(ErrorDescriptions.TheScopesNeedToBeSpecified, "openid"),
                    authorizationCodeGrantTypeParameter.State);
            }

            var prompts = _parameterParserHelper.ParsePromptParameters(authorizationCodeGrantTypeParameter.Prompt);
            var result = ProcessPromptParameters(
                prompts, 
                claimsPrincipal, 
                authorizationCodeGrantTypeParameter,
                code);
            return result;
        }

        /// <summary>
        /// Process the prompt parameter.
        /// </summary>
        /// <param name="prompts">Prompt parameter values</param>
        /// <param name="claimsPrincipal">User's claims</param>
        /// <param name="authorizationCodeGrantTypeParameter">Authorization code grant-type parameter</param>
        /// <param name="code">Encrypted and signed authorization code grant-type parameter</param>
        /// <returns>The action result interpreted by the controller</returns>
        private ActionResult ProcessPromptParameters(
            IList<PromptParameter> prompts,
            IPrincipal claimsPrincipal,
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            string code)
        {
            var endUserIsAuthenticated = claimsPrincipal.Identity.IsAuthenticated;
            // Raise "login_required" exception : if the prompt parameter is "none" AND the user is not authenticated
            // Raise "interaction_required" exception : if there's no consent from the user.
            if (prompts.Contains(PromptParameter.none))
            {
                if (!endUserIsAuthenticated)
                {
                    throw new IdentityServerExceptionWithState(
                        ErrorCodes.LoginRequiredCode,
                        ErrorDescriptions.TheUserNeedsToBeAuthenticated,
                        authorizationCodeGrantTypeParameter.State);
                }

                var principal = claimsPrincipal as ClaimsPrincipal;
                var subject = principal.GetSubject();
                var consents = _consentRepository.GetConsentsForGivenUser(subject);
                Consent confirmedConsent = null;
                if (consents != null && consents.Any())
                {
                    var scopeNames =
                        _parameterParserHelper.ParseScopeParameters(authorizationCodeGrantTypeParameter.Scope);
                    confirmedConsent = consents.FirstOrDefault(
                        c =>
                            c.Client.ClientId == authorizationCodeGrantTypeParameter.ClientId &&
                            c.GrantedScopes != null && c.GrantedScopes.Any() &&
                            c.GrantedScopes.All(s => scopeNames.Contains(s.Name)));
                }

                if (confirmedConsent == null)
                {
                    throw new IdentityServerExceptionWithState(
                            ErrorCodes.InteractionRequiredCode,
                            ErrorDescriptions.TheUserNeedsToGiveIsConsent,
                            authorizationCodeGrantTypeParameter.State);
                }

                var authorizationCode = new AuthorizationCode()
                {
                    CreateDateTime = DateTime.UtcNow,
                    Consent = confirmedConsent,
                    Value = Guid.NewGuid().ToString()
                };
                _authorizationCodeRepository.AddAuthorizationCode(authorizationCode);
                var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                result.RedirectInstruction.AddParameter("code", authorizationCode.Value);
                return result;
            }

            // Redirects to the authentication screen 
            // if the "prompt" parameter is equal to "login" OR
            // The user is not authenticated AND the prompt parameter is different from "none"
            if (prompts.Contains(PromptParameter.login))
            {
                var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                result.RedirectInstruction.AddParameter("code", code);
                result.RedirectInstruction.Action = IdentityServerEndPoints.AuthenticateIndex;
                return result;
            }

            if (prompts.Contains(PromptParameter.consent))
            {
                var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
                result.RedirectInstruction.AddParameter("code", code);
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
    }
}
