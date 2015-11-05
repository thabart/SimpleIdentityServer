using System.Security.Claims;
using System.Security.Principal;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt.Generator;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.Api.Authorization.Actions
{
    public interface IGetTokenViaImplicitWorkflowOperation
    {
        ActionResult Execute(
            AuthorizationParameter authorizationParameter,
            IPrincipal principal,
            string code);
    }

    public class GetTokenViaImplicitWorkflowOperation : IGetTokenViaImplicitWorkflowOperation
    {
        private readonly IProcessAuthorizationRequest _processAuthorizationRequest;

        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IGrantedTokenRepository _grantedTokenRepository;

        private readonly ITokenHelper _tokenHelper;

        private readonly IJwtGenerator _jwtGenerator;

        public GetTokenViaImplicitWorkflowOperation(
            IProcessAuthorizationRequest processAuthorizationRequest,
            IParameterParserHelper parameterParserHelper,
            IGrantedTokenRepository grantedTokenRepository,
            ITokenHelper tokenHelper,
            IJwtGenerator jwtGenerator)
        {
            _processAuthorizationRequest = processAuthorizationRequest;
            _parameterParserHelper = parameterParserHelper;
            _grantedTokenRepository = grantedTokenRepository;
            _tokenHelper = tokenHelper;
            _jwtGenerator = jwtGenerator;
        }

        public ActionResult Execute(
            AuthorizationParameter authorizationParameter,
            IPrincipal principal,
            string code)
        {
            if (string.IsNullOrWhiteSpace(authorizationParameter.Nonce))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "nonce"),
                    authorizationParameter.State);
            }

            var result = _processAuthorizationRequest.Process(
                authorizationParameter,
                principal,
                code);

            if (result.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var claimsPrincipal = principal as ClaimsPrincipal;
                if (claimsPrincipal == null)
                {
                    return result;
                }

                var responses = _parameterParserHelper.ParseResponseType(authorizationParameter.ResponseType);
                if (responses.Contains(ResponseType.id_token))
                {
                    var jwtClaims = _jwtGenerator.GenerateJwtClaims(claimsPrincipal, authorizationParameter);
                }

                if (responses.Contains(ResponseType.token))
                {
                    var allowedTokenScopes = string.Empty;
                    if (!string.IsNullOrWhiteSpace(authorizationParameter.Scope))
                    {
                        allowedTokenScopes = string.Join(" ", _parameterParserHelper.ParseScopeParameters(authorizationParameter.Scope));
                    }

                    var generatedToken = _tokenHelper.GenerateToken(allowedTokenScopes);
                    _grantedTokenRepository.Insert(generatedToken);
                    result.RedirectInstruction.AddParameter("access_token", generatedToken.AccessToken);
                }
            }

            return result;
        }
    }
}
