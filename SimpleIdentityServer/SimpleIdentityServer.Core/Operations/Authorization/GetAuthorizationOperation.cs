using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;

using System;
using System.Net.Http;

namespace SimpleIdentityServer.Core.Operations.Authorization
{
    public interface IGetAuthorizationOperation
    {
        HttpResponseMessage Execute(GetAuthorizationParameter parameter);
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
        
        public HttpResponseMessage Execute(GetAuthorizationParameter parameter)
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

            var response = new HttpResponseMessage();
            var prompts = parameter.GetPromptParameters();
            if (prompts.Contains(PromptParameter.login))
            {
                // TODO : Prompt for login.
                response.StatusCode = System.Net.HttpStatusCode.Moved;
                response.Headers.Location = new Uri("~/Authorize");
            }

            if (prompts.Contains(PromptParameter.none))
            {
                // TODO : error occured if the end-user is not already authenticated : login_required
                // TODO : error occured if the client does not have pre-configured consent for the requested claims.
            }

            return response;
        }
    }
}
