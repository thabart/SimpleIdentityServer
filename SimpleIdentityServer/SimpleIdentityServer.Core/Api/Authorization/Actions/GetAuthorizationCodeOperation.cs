using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;

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

        private readonly IAuthorizationCodeRepository _authorizationCodeRepository;

        private readonly IConsentRepository _consentRepository;

        private readonly IParameterParserHelper _parameterParserHelper;

        public GetAuthorizationCodeOperation(
            IProcessAuthorizationRequest processAuthorizationRequest,
            IAuthorizationCodeRepository authorizationCodeRepository,
            IConsentRepository consentRepository,
            IParameterParserHelper parameterParserHelper)
        {
            _processAuthorizationRequest = processAuthorizationRequest;
            _authorizationCodeRepository = authorizationCodeRepository;
            _consentRepository = consentRepository;
            _parameterParserHelper = parameterParserHelper;
        }

        public ActionResult Execute(
            AuthorizationParameter authorizationParameter,
            IPrincipal claimsPrincipal,
            string code)
        {
            var result = _processAuthorizationRequest.Process(authorizationParameter,
                claimsPrincipal,
                code);
            if (result.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var confirmedConsent = GetResourceOwnerConsent(claimsPrincipal, authorizationParameter);
                var authorizationCode = new AuthorizationCode()
                {
                    CreateDateTime = DateTime.UtcNow,
                    Consent = confirmedConsent,
                    Value = Guid.NewGuid().ToString()
                };

                _authorizationCodeRepository.AddAuthorizationCode(authorizationCode);
                result.RedirectInstruction.AddParameter("code", authorizationCode.Value);
            }

            return result;
        }

        private Consent GetResourceOwnerConsent(
            IPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            var principal = claimsPrincipal as ClaimsPrincipal;
            var subject = principal.GetSubject();
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
