using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Configuration
{
    public interface IConfigurationClient
    {
        Task<ConfigurationResponse> GetConfiguration(Uri wellKnownConfigurationUri);
    }

    internal sealed class ConfigurationClient : IConfigurationClient
    {
        private readonly IGetConfigurationOperation _getConfigurationOperation;

        public ConfigurationClient(IGetConfigurationOperation getConfigurationOperation)
        {
            _getConfigurationOperation = getConfigurationOperation;
        }

        public Task<ConfigurationResponse> GetConfiguration(Uri wellKnownConfigurationUri)
        {
            return _getConfigurationOperation.ExecuteAsync(wellKnownConfigurationUri);
        }
    }
}
