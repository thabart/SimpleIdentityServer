using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
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

        public GetTokenViaImplicitWorkflowOperation(
            IProcessAuthorizationRequest processAuthorizationRequest,
            IParameterParserHelper parameterParserHelper,
            IGrantedTokenRepository grantedTokenRepository,
            ITokenHelper tokenHelper)
        {
            _processAuthorizationRequest = processAuthorizationRequest;
            _parameterParserHelper = parameterParserHelper;
            _grantedTokenRepository = grantedTokenRepository;
            _tokenHelper = tokenHelper;
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
                if (responses.Contains(ResponseTypeParameter.id_token))
                {
                    // TODO : generate id_token

                    // TODO : Provide the possibility to configure the the issuer of the response.
                    var audiences = new List<string>();
                    var claims = new List<Claim>
                    {
                        new Claim(Constants.StandardClaimNames.Subject, claimsPrincipal.GetSubject())
                    };

                    // TODO : the claim "azp" is only needed when the ID token has a single audience value & that audience is different than the authorized party.


                    var jwtClaims = new JwtClaims
                    {
                        iss = "http://localhost/simpleidentityserver",
                        Claims = claims
                    };
                }

                if (responses.Contains(ResponseTypeParameter.token))
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
