using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Services
{
    internal sealed class DefaultConfigurationService : IConfigurationService
    {
        private readonly OAuthConfigurationOptions _options;

        public DefaultConfigurationService(OAuthConfigurationOptions options)
        {
            _options = options == null ? new OAuthConfigurationOptions() : _options;
        }

        public Task<string> DefaultLanguageAsync()
        {
            return Task.FromResult(_options.DefaultLanguage);
        }

        public Task<double> GetAuthorizationCodeValidityPeriodInSecondsAsync()
        {
            return Task.FromResult(_options.AuthorizationCodeValidityPeriodInSeconds);
        }

        public Task<double> GetTokenValidityPeriodInSecondsAsync()
        {
            return Task.FromResult(_options.TokenValidityPeriodInSeconds);
        }
    }
}
