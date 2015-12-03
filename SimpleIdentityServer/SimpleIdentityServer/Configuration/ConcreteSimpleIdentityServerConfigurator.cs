using System;
using System.Web;
using System.Web.Hosting;
using SimpleIdentityServer.Core.Configuration;

namespace SimpleIdentityServer.Api.Configuration
{
    public class ConcreteSimpleIdentityServerConfigurator : ISimpleIdentityServerConfigurator
    {
        public string GetIssuerName()
        {
            var request = HttpContext.Current.Request;
            var scheme = request.Url.Scheme;
            var authority = request.Url.Authority;
            var virtualPathRoot = HostingEnvironment.ApplicationVirtualPath;
            return scheme + "://" + authority + virtualPathRoot;
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