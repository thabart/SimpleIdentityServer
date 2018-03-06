using Microsoft.AspNetCore.Http;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Uma.Host.Extensions;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Host.Tests.Services
{
    public class DefaultConfigurationService : IConfigurationService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public DefaultConfigurationService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public Task<string> DefaultLanguageAsync()
        {
            return Task.FromResult("en");
        }

        public Task<double> GetAuthorizationCodeValidityPeriodInSecondsAsync()
        {
            double result = 3600;
            return Task.FromResult(result);
        }

        public Task<string> GetIssuerNameAsync()
        {
            var request = _contextAccessor.HttpContext.Request;
            return Task.FromResult(request.GetAbsoluteUriWithVirtualPath());
        }

        public Task<double> GetTokenValidityPeriodInSecondsAsync()
        {
            double result = 3600;
            return Task.FromResult(result);
        }
    }
}
