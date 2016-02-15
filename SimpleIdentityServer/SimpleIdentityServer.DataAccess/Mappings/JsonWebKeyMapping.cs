using System.Data.Entity.ModelConfiguration;

using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public sealed class JsonWebKeyMapping : EntityTypeConfiguration<JsonWebKey>
    {
        public JsonWebKeyMapping()
        {
            ToTable("jsonWebKeys");
            HasKey(j => j.Kid);
            Property(j => j.Kty);
            Property(j => j.Use);
            Property(j => j.KeyOps);
            Property(j => j.Alg);
            Property(j => j.X5u);
            Property(j => j.X5t);
            Property(j => j.X5tS256);
            Property(j => j.SerializedKey);
        }
    }
}
