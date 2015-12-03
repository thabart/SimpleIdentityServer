using System;

namespace SimpleIdentityServer.Core.Configuration
{
    public interface ISimpleIdentityServerConfigurator
    {
        string GetIssuerName();

        double GetTokenValidityPeriodInSeconds();

        double GetAuthorizationCodeValidityPeriodInSeconds();

        string DefaultLanguage();
    }

    public class SimpleIdentityServerConfigurator : ISimpleIdentityServerConfigurator
    {
        /// <summary>
        /// Returns the issuer name.
        /// This value is used in the JWT claims.
        /// </summary>
        /// <returns>Issuer name</returns>
        public string GetIssuerName()
        {
            return "http://localhost/identity";
        }

        /// <summary>
        /// Returns the validity of an access token or identity token in seconds
        /// </summary>
        /// <returns>Validity of an access token or identity token in seconds</returns>
        public double GetTokenValidityPeriodInSeconds()
        {
            return 3000000;
        }

        /// <summary>
        /// Returns the validity period of an authorization token in seconds.
        /// </summary>
        /// <returns>Validity period is seconds</returns>
        public double GetAuthorizationCodeValidityPeriodInSeconds()
        {
            return 3000000;
        }

        public string DefaultLanguage()
        {
            return "en";
        }
    }
}
