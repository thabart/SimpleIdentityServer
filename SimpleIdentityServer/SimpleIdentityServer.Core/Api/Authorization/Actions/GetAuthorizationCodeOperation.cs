using System.Security.Claims;
using System.Security.Principal;

using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Errors;

namespace SimpleIdentityServer.Core.Api.Authorization.Actions
{
    public interface IGetAuthorizationCodeOperation
    {
        ActionResult Execute(
            AuthorizationParameter authorizationParameter, 
            IPrincipal claimsPrincipal,
            string code);
    }

    public class GetAuthorizationCodeOperation : IGetAuthorizationCodeOperation
    {
        private readonly IProcessAuthorizationRequest _processAuthorizationRequest;

        private IClientValidator _clientValidator;

        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;

        public GetAuthorizationCodeOperation(
            IProcessAuthorizationRequest processAuthorizationRequest,
            IClientValidator clientValidator,
            IGenerateAuthorizationResponse generateAuthorizationResponse)
        {
            _processAuthorizationRequest = processAuthorizationRequest;
            _clientValidator = clientValidator;
            _generateAuthorizationResponse = generateAuthorizationResponse;
        }

        public ActionResult Execute(
            AuthorizationParameter authorizationParameter,
            IPrincipal claimsPrincipal,
            string code)
        {
            var result = _processAuthorizationRequest.Process(authorizationParameter,
                claimsPrincipal,
                code);
            var client = _clientValidator.ValidateClientExist(authorizationParameter.ClientId);
            if (!_clientValidator.ValidateGrantType(GrantType.authorization_code, client))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestUriCode,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        "authorization_code"),
                    authorizationParameter.State);
            }

            if (result.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var principal = claimsPrincipal as ClaimsPrincipal;
                if (principal == null)
                {
                    return result;
                }

                _generateAuthorizationResponse.Execute(result, authorizationParameter,
                    principal);
            }

            return result;
        }
    }
}
