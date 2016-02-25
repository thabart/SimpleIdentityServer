using SimpleIdentityServer.DataAccess.SqlServer.Models;
using Microsoft.Data.Entity;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public static class JsonWebKeyMapping
    {
        public static void AddJsonWebKeyMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JsonWebKey>()
                .ToTable("jsonWebKeys")
                .HasKey(j => j.Kid);
            modelBuilder.Entity<JsonWebKey>()
                .HasOne(c => c.Client)
                .WithMany(c => c.JsonWebKeys)
                .HasForeignKey(c => c.ClientId);
            /*
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
            */
        }
    }
}
