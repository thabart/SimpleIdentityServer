using System;
using System.Linq;
using System.Security.Claims;

using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Jwt.Encrypt;

namespace SimpleIdentityServer.Core.Common
{
    public interface IGenerateAuthorizationResponse
    {
        void Execute(
            ActionResult actionResult,
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal);
    }

    public class GenerateAuthorizationResponse : IGenerateAuthorizationResponse
    {
        private readonly IAuthorizationCodeRepository _authorizationCodeRepository;

        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IJwtGenerator _jwtGenerator;

        private readonly ITokenHelper _tokenHelper;

        private readonly IGrantedTokenRepository _grantedTokenRepository;

        private readonly IConsentRepository _consentRepository;

        public GenerateAuthorizationResponse(
            IAuthorizationCodeRepository authorizationCodeRepository,
            IParameterParserHelper parameterParserHelper,
            IJwtGenerator jwtGenerator,
            ITokenHelper tokenHelper,
            IGrantedTokenRepository grantedTokenRepository,
            IConsentRepository consentRepository,
            IJweGenerator jweGenerator)
        {
            _authorizationCodeRepository = authorizationCodeRepository;
            _parameterParserHelper = parameterParserHelper;
            _jwtGenerator = jwtGenerator;
            _tokenHelper = tokenHelper;
            _grantedTokenRepository = grantedTokenRepository;
            _consentRepository = consentRepository;
        }

        public void Execute(
            ActionResult actionResult, 
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal)
        {
            var responses = _parameterParserHelper.ParseResponseType(authorizationParameter.ResponseType);
            if (responses.Contains(ResponseType.id_token))
            {
                var jwsPayLoad = _jwtGenerator.GenerateJwsPayload(claimsPrincipal, authorizationParameter);
                var idToken = _jwtGenerator.Sign(jwsPayLoad, authorizationParameter);
                idToken = _jwtGenerator.Encrypt(idToken, authorizationParameter);
                actionResult.RedirectInstruction.AddParameter("id_token", idToken);
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
                actionResult.RedirectInstruction.AddParameter("access_token", generatedToken.AccessToken);
            }

            if (responses.Contains(ResponseType.code))
            {
                var assignedConsent = GetResourceOwnerConsent(claimsPrincipal, authorizationParameter);
                if (assignedConsent != null)
                {
                    var jwsPayLoad = _jwtGenerator.GenerateJwsPayload(claimsPrincipal, authorizationParameter);
                    var idToken = _jwtGenerator.Sign(jwsPayLoad, authorizationParameter);
                    idToken = _jwtGenerator.Encrypt(idToken, authorizationParameter);
                    var authorizationCode = new AuthorizationCode
                    {
                        Code = Guid.NewGuid().ToString(),
                        RedirectUri = authorizationParameter.RedirectUrl,
                        CreateDateTime = DateTime.UtcNow,
                        ClientId = authorizationParameter.ClientId,
                        IdToken = idToken,
                        Scopes = authorizationParameter.Scope
                    };

                    _authorizationCodeRepository.AddAuthorizationCode(authorizationCode);
                    actionResult.RedirectInstruction.AddParameter("code", authorizationCode.Code);
                }
            }

            if (!string.IsNullOrWhiteSpace(authorizationParameter.State))
            {
                actionResult.RedirectInstruction.AddParameter("state", authorizationParameter.State);
            }
        }
        
        private Consent GetResourceOwnerConsent(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            var subject = claimsPrincipal.GetSubject();
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
    }
}
