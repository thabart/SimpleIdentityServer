using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Stores
{
    public interface IMappingStore
    {
        Task<AdMapping> GetMapping(string attributeId);
        Task<List<AdMapping>> GetAll();
        Task<bool> AddMapping(AdMapping adMapping);
        Task<bool> Remove(string attributeId);
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

        public async Task<bool> AddMapping(AdMapping adMapping)
        {
            if(adMapping == null)
            {
                throw new ArgumentNullException(nameof(adMapping));
            }

            adMapping.CreateDateTime = DateTime.UtcNow;
            _context.Mappings.Add(adMapping);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public Task<List<AdMapping>> GetAll()
        {
            return _context.Mappings.ToListAsync();
        }

        public async Task<bool> Remove(string attributeId)
        {
            if (string.IsNullOrWhiteSpace(attributeId))
            {
                throw new ArgumentNullException(nameof(attributeId));
            }

            var record = await _context.Mappings.FirstOrDefaultAsync(m => m.AttributeId == attributeId).ConfigureAwait(false);
            if (record == null)
            {
                return false;
            }

            _context.Mappings.Remove(record);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }
}
