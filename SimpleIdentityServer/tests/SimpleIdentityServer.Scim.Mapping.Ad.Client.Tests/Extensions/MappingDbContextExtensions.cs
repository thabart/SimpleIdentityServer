using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using System;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Tests.Extensions
{
    internal static class MappingDbContextExtensions
    {
        public static void EnsureSeedData(this MappingDbContext context)
        {
            context.Mappings.AddRange(new[]
            {
                new AdMapping
                {
                    AdPropertyName = "property",
                    AttributeId = "attributeid",
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                }
            });
            context.SaveChanges();
        }
    }
}
