using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.Operations.Authorization
{
    public interface IGetAuthorizationOperation
    {
        AuthorizationResult Execute(GetAuthorizationParameter parameter, ClaimsPrincipal claimsPrincipal);
    }

    public class GetAuthorizationOperation : IGetAuthorizationOperation
    {
        private readonly ITokenHelper _tokenHelper;

        private readonly IScopeValidator _scopeValidator;

        private readonly IClientValidator _clientValidator;

        public GetAuthorizationOperation(
            ITokenHelper tokenHelper,
            IClientValidator clientValidator,
            IScopeValidator scopeValidator)
        {
            _tokenHelper = tokenHelper;
            _clientValidator = clientValidator;
            _scopeValidator = scopeValidator;
        }
        
        public AuthorizationResult Execute(GetAuthorizationParameter parameter, ClaimsPrincipal claimsPrincipal)
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

            var prompts = parameter.GetPromptParameters();
            var result = new AuthorizationResult();
            AuthenticateEndUser(prompts, result, claimsPrincipal, parameter);
            return result;
        }

        /// <summary>
        /// Tries to authenticate the user.
        /// OPEN-ID-URL : http://openid.net/specs/openid-connect-core-1_0.html#Authenticates
        /// </summary>
        private void AuthenticateEndUser(
            IList<PromptParameter> promptParameters,
            AuthorizationResult result,
            ClaimsPrincipal claimsPrincipal,
            GetAuthorizationParameter parameter)
        {
            var endUserIsAuthenticated = claimsPrincipal.Identity.IsAuthenticated;
            if (promptParameters.Contains(PromptParameter.login)
                || (!endUserIsAuthenticated && !promptParameters.Contains(PromptParameter.none)))
            {
                result.Redirection = Redirection.Authorize;
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
