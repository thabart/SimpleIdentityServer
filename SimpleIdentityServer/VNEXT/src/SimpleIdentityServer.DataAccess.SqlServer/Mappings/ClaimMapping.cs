using System.Data.Entity.ModelConfiguration;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public sealed class ClaimMapping : EntityTypeConfiguration<Claim>
    {
        public ClaimMapping()
        {
            ToTable("claims");
            HasKey(p => p.Code);
        }
    }
}
