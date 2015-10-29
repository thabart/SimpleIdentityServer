using System.Collections.Generic;
using System.Security.Principal;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.Api.Authorization.Actions
{
    public interface IGetAuthorizationCodeOperation
    {
        ActionResult Execute(AuthorizationCodeGrantTypeParameter parameter, IPrincipal claimsPrincipal);
    }

    public class GetAuthorizationCodeOperation : IGetAuthorizationCodeOperation
    {

        private readonly IScopeValidator _scopeValidator;

        private readonly IClientValidator _clientValidator;

        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IActionResultFactory _actionResultFactory;

        public GetAuthorizationCodeOperation(
            IClientValidator clientValidator,
            IScopeValidator scopeValidator,
            IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory)
        {
            _clientValidator = clientValidator;
            _scopeValidator = scopeValidator;
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
        }

        public ActionResult Execute(
            AuthorizationCodeGrantTypeParameter parameter,
            IPrincipal claimsPrincipal)
        {
            parameter.Validate();
            var client = _clientValidator.ValidateClientExist(parameter.ClientId);
            _clientValidator.ValidateRedirectionUrl(parameter.RedirectUrl, client);
            var allowedScopes = _scopeValidator.ValidateAllowedScopes(parameter.Scope, client);
            if (!allowedScopes.Contains("openid"))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestUriCode,
                    string.Format(ErrorDescriptions.TheScopesNeedToBeSpecified, "openid"),
                    parameter.State);
            }

            var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
            var prompts = _parameterParserHelper.ParsePromptParameters(parameter.Prompt);
            AuthenticateEndUser(prompts, result, claimsPrincipal, parameter);
            return result;
        }

        /// <summary>
        /// Tries to authenticate the user.
        /// OPEN-ID-URL : http://openid.net/specs/openid-connect-core-1_0.html#Authenticates
        /// </summary>
        private void AuthenticateEndUser(
            IList<PromptParameter> promptParameters,
            ActionResult result,
            IPrincipal claimsPrincipal,
            AuthorizationCodeGrantTypeParameter parameter)
        {
            var endUserIsAuthenticated = claimsPrincipal.Identity.IsAuthenticated;
            if (promptParameters.Contains(PromptParameter.login)
                || (!endUserIsAuthenticated && !promptParameters.Contains(PromptParameter.none)))
            {
                result.RedirectInstruction.Action = IdentityServerEndPoints.AuthenticateIndex;
            }

            if (promptParameters.Contains(PromptParameter.none) && !endUserIsAuthenticated)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.LoginRequiredCode,
                    ErrorDescriptions.TheUserNeedsToBeAuthenticated,
                    parameter.State);
            }

            if (promptParameters.Contains(PromptParameter.none))
            {
                // TODO : error occured if the client does not have pre-configured consent for the requested claims.
            }
        }
    }
}
