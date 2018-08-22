using Microsoft.EntityFrameworkCore;
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
        private readonly MappingDbContext _context;

        public ConfigurationStore(MappingDbContext context)
        {
            _context = context;
        }

        public Task<AdConfiguration> GetConfiguration()
        {
            return _context.Configurations.FirstOrDefaultAsync();
        }
    }
}