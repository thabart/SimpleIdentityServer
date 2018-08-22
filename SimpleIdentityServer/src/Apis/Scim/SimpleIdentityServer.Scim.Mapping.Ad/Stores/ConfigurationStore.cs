using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Stores
{
    public interface IConfigurationStore
    {
        Task<AdConfiguration> GetConfiguration();
    }

    internal sealed class ConfigurationStore : IConfigurationStore
    {
        public Task<AdConfiguration> GetConfiguration()
        {
            return null;
        }
    }
}