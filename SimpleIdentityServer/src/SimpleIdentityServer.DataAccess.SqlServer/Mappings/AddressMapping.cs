using Microsoft.Data.Entity;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public static class AddressMapping
    {
        public static void AddAddressMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>()
                .ToTable("addresses")
                .HasKey(a => a.Id);
            /*
            ToTable("addresses");
            HasKey(a => a.Id);
            Property(a => a.Formatted);
            Property(a => a.StreetAddress);
            Property(a => a.Locality);
            Property(a => a.Region);
            Property(a => a.PostalCode);
            Property(a => a.Country);
            */
        }
    }
}
