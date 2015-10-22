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
        AuthorizationResult Execute(GetAuthorizationParameter parameter);
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
        
        public AuthorizationResult Execute(GetAuthorizationParameter parameter)
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
            if (prompts.Contains(PromptParameter.login))
            {
                result.Redirection = Redirection.Authorize;
            }

            if (prompts.Contains(PromptParameter.none))
            {
                // TODO : error occured if the end-user is not already authenticated : login_required
                // TODO : error occured if the client does not have pre-configured consent for the requested claims.
            }

            return result;
        }
    }
}
