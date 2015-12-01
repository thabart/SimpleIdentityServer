using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IConsentHelper
    {
        Consent GetConsentConfirmedByResourceOwner(
            string subject,
            AuthorizationParameter authorizationParameter);
    }

    public class ConsentHelper : IConsentHelper
    {
        private readonly IConsentRepository _consentRepository;

        private readonly IParameterParserHelper _parameterParserHelper;

        public ConsentHelper(
            IConsentRepository consentRepository,
            IParameterParserHelper parameterParserHelper)
        {
            _consentRepository = consentRepository;
            _parameterParserHelper = parameterParserHelper;
        }

        public Consent GetConsentConfirmedByResourceOwner(
            string subject,
            AuthorizationParameter authorizationParameter)
        {
            var consents = _consentRepository.GetConsentsForGivenUser(subject);
            Consent confirmedConsent = null;
            if (consents != null && consents.Any())
            {
                var claimsParameter = authorizationParameter.Claims;
                if (claimsParameter.IsAnyUserInfoClaimParameter() ||
                    claimsParameter.IsAnyIdentityTokenClaimParameter())
                {
                    var expectedClaims = claimsParameter.GetClaimNames();
                    confirmedConsent = consents.FirstOrDefault(
                        c =>
                            c.Client.ClientId == authorizationParameter.ClientId &&
                            c.Claims != null && c.Claims.Any() &&
                            c.Claims.All(cl => expectedClaims.Contains(cl)));
                }
                else
                {
                    var scopeNames =
                        _parameterParserHelper.ParseScopeParameters(authorizationParameter.Scope);
                    confirmedConsent = consents.FirstOrDefault(
                        c =>
                            c.Client.ClientId == authorizationParameter.ClientId &&
                            c.GrantedScopes != null && c.GrantedScopes.Any() &&
                            c.GrantedScopes.All(s => scopeNames.Contains(s.Name)));
                }
            }

            return confirmedConsent;
        }
    }
}
