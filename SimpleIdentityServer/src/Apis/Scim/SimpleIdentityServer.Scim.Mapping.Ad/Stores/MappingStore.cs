using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Stores
{
    public interface IMappingStore
    {
        Task<AdMapping> GetMapping(string attributeId);
    }

    internal sealed class MappingStore : IMappingStore
    {
        private readonly MappingDbContext _context;

        public MappingStore(MappingDbContext context)
        {
            _context = context;
        }

        public Task<AdMapping> GetMapping(string attributeId)
        {
            return _context.Mappings.FirstOrDefaultAsync(s => s.AttributeId == attributeId);
        }
    }
}
