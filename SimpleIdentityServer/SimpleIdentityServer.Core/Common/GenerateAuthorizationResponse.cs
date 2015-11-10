using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;

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

        private readonly IJwsGenerator _jwsGenerator;

        private readonly ITokenHelper _tokenHelper;

        private readonly IGrantedTokenRepository _grantedTokenRepository;

        private readonly IConsentRepository _consentRepository;

        public GenerateAuthorizationResponse(
            IAuthorizationCodeRepository authorizationCodeRepository,
            IParameterParserHelper parameterParserHelper,
            IJwsGenerator jwsGenerator,
            ITokenHelper tokenHelper,
            IGrantedTokenRepository grantedTokenRepository,
            IConsentRepository consentRepository)
        {
            _authorizationCodeRepository = authorizationCodeRepository;
            _parameterParserHelper = parameterParserHelper;
            _jwsGenerator = jwsGenerator;
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
                var jwsPayLoad = _jwsGenerator.GenerateJwsPayload(claimsPrincipal, authorizationParameter);
                var idToken = _jwsGenerator.GenerateJws(jwsPayLoad, authorizationParameter);
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
                    var authorizationCode = new AuthorizationCode
                    {
                        CreateDateTime = DateTime.UtcNow,
                        Consent = assignedConsent,
                        Value = Guid.NewGuid().ToString()
                    };
                    _authorizationCodeRepository.AddAuthorizationCode(authorizationCode);
                    actionResult.RedirectInstruction.AddParameter("code", authorizationCode.Value);
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
