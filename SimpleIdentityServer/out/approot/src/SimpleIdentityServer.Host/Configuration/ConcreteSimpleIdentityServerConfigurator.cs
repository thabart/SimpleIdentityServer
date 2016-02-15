using System;
using SimpleIdentityServer.Core.Configuration;
using Microsoft.AspNet.Http;
using SimpleIdentityServer.Host.Extensions;

namespace SimpleIdentityServer.Host.Configuration
{
    public class ConcreteSimpleIdentityServerConfigurator : ISimpleIdentityServerConfigurator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConcreteSimpleIdentityServerConfigurator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetIssuerName()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var result = request.GetAbsoluteUriWithVirtualPath();
            return result;
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