using Microsoft.Data.Entity;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public static class GrantedTokenMapping
    {
        public static void AddGrantedTokenMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GrantedToken>()
                .ToTable("grantedTokens")
                .HasKey(c => c.Id);
            /*
            ToTable("grantedTokens");
            HasKey(g => g.Id);
            Property(g => g.AccessToken);
            Property(g => g.RefreshToken);
            Property(g => g.Scope);
            Property(g => g.ExpiresIn);
            Property(g => g.CreateDateTime);
            Property(g => g.ClientId);
            Property(g => g.IdTokenPayLoad);
            Property(g => g.UserInfoPayLoad);
            */
        }
    }
}
