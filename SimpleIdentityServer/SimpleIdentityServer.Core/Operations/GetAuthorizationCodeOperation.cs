using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.Operations
{
    public interface IGetAuthorizationCodeOperation
    {

    }

    public class GetAuthorizationCodeOperation : IGetAuthorizationCodeOperation
    {
        private readonly ITokenHelper _tokenHelper;

        private readonly IScopeValidator _scopeValidator;
        
        private readonly IClientValidator _clientValidator;

        public GetAuthorizationCodeOperation(
            ITokenHelper tokenHelper,
            IClientValidator clientValidator,
            IScopeValidator scopeValidator)
        {
            _tokenHelper = tokenHelper;
            _clientValidator = clientValidator;
            _scopeValidator = scopeValidator;
        }

        public void Execute(GetAuthorizationCodeParameter parameter)
        {
            try
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

            }
            catch (IdentityServerException ex)
            {
                throw new IdentityServerExceptionWithState(ex, parameter.State);
            }
        }
    }
}
