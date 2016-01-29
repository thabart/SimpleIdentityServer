using System;
using SimpleIdentityServer.Core.Configuration;

namespace SimpleIdentityServer.Api.Tests.Common.Fakes
{
    public class FakeSimpleIdentityServerConfigurator : ISimpleIdentityServerConfigurator
    {
        public string Issuer { private get; set; }

        public int TokenValidityPeriod { private get; set; }

        public int AuthorizationCodeValidityPeriod { private get; set; }

        public double GetAuthorizationCodeValidityPeriodInSeconds()
        {
            return AuthorizationCodeValidityPeriod;
        }

        public string GetIssuerName()
        {
            return Issuer;
        }

        public double GetTokenValidityPeriodInSeconds()
        {
            return TokenValidityPeriod;
        }

        public string DefaultLanguage()
        {
            return "en";
        }
    }
}
