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

namespace SimpleIdentityServer.Core.Api.Authorization.Common
{
    public interface IProcessAuthorizationRequest
    {
        ActionResult Process(
            AuthorizationParameter authorizationParameter,
            IPrincipal claimsPrincipal,
            string code);
    }

    public class ProcessAuthorizationRequest : IProcessAuthorizationRequest
    {
        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IClientValidator _clientValidator;

        private readonly IScopeValidator _scopeValidator;

        private readonly IActionResultFactory _actionResultFactory;

        private readonly IConsentRepository _consentRepository;

        public ProcessAuthorizationRequest(
            IParameterParserHelper parameterParserHelper,
            IClientValidator clientValidator,
            IScopeValidator scopeValidator,
            IActionResultFactory actionResultFactory,
            IConsentRepository consentRepository)
        {
            _parameterParserHelper = parameterParserHelper;
            _clientValidator = clientValidator;
            _scopeValidator = scopeValidator;
            _actionResultFactory = actionResultFactory;
            _consentRepository = consentRepository;
        }

        public ActionResult Process(
            AuthorizationParameter authorizationParameter, 
            IPrincipal claimsPrincipal, 
            string code)
        {
            var prompts = _parameterParserHelper.ParsePromptParameters(authorizationParameter.Prompt);
            if (prompts == null || !prompts.Any())
            {
                prompts = new List<PromptParameter>();
                var endUserIsAuthenticated = IsAuthenticated(claimsPrincipal);
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
            _clientValidator.ValidateRedirectionUrl(authorizationParameter.RedirectUrl, client);
            var allowedScopes = _scopeValidator.ValidateAllowedScopes(authorizationParameter.Scope, client);
            if (!allowedScopes.Contains("openid"))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestUriCode,
                    string.Format(ErrorDescriptions.TheScopesNeedToBeSpecified, "openid"),
                    authorizationParameter.State);
            }

            var result = ProcessPromptParameters(
                prompts,
                claimsPrincipal,
                authorizationParameter,
                code);
            return result;
        }

        /// <summary>
        /// Process the prompt authorizationParameter.
        /// </summary>
        /// <param name="prompts">Prompt authorizationParameter values</param>
        /// <param name="claimsPrincipal">User's claims</param>
        /// <param name="authorizationParameter">Authorization code grant-type authorizationParameter</param>
        /// <param name="code">Encrypted and signed authorization code grant-type authorizationParameter</param>
        /// <returns>The action result interpreted by the controller</returns>
        private ActionResult ProcessPromptParameters(
            IList<PromptParameter> prompts,
            IPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter,
            string code)
        {
            var endUserIsAuthenticated = IsAuthenticated(claimsPrincipal);
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

                var confirmedConsent = GetResourceOwnerConsent(claimsPrincipal, authorizationParameter);
                if (confirmedConsent == null)
                {
                    throw new IdentityServerExceptionWithState(
                            ErrorCodes.InteractionRequiredCode,
                            ErrorDescriptions.TheUserNeedsToGiveIsConsent,
                            authorizationParameter.State);
                }

                var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                if (!string.IsNullOrWhiteSpace(authorizationParameter.State))
                {
                    result.RedirectInstruction.AddParameter("state", authorizationParameter.State);
                }

                return result;
            }

            // Redirects to the authentication screen 
            // if the "prompt" authorizationParameter is equal to "login" OR
            // The user is not authenticated AND the prompt authorizationParameter is different from "none"
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

        private Consent GetResourceOwnerConsent(
            IPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            var principal = claimsPrincipal as ClaimsPrincipal;
            var subject = principal.GetSubject();
            var consents = _consentRepository.GetConsentsForGivenUser(subject);
            Consent confirmedConsent = null;
            if (consents != null && consents.Any())
            {
                var scopeNames =
                    _parameterParserHelper.ParseScopeParameters(authorizationParameter.Scope);
                confirmedConsent = consents.FirstOrDefault(
                    c =>
                        c.Client.ClientId == authorizationParameter.ClientId &&
                        c.GrantedScopes != null && c.GrantedScopes.Any() &&
                        c.GrantedScopes.All(s => scopeNames.Contains(s.Name)));
            }

            return confirmedConsent;
        }

        private static bool IsAuthenticated(IPrincipal principal)
        {
            return principal == null || principal.Identity == null ?
                false :
                principal.Identity.IsAuthenticated;
        }
    }
}
