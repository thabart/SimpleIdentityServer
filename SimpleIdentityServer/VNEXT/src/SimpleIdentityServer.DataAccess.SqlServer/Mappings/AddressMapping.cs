using System.Data.Entity.ModelConfiguration;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public sealed class AddressMapping : EntityTypeConfiguration<Address>
    {
        public AddressMapping()
        {
            ToTable("addresses");
            HasKey(a => a.Id);
            Property(a => a.Formatted);
            Property(a => a.StreetAddress);
            Property(a => a.Locality);
            Property(a => a.Region);
            Property(a => a.PostalCode);
            Property(a => a.Country);
        }
    }
}
