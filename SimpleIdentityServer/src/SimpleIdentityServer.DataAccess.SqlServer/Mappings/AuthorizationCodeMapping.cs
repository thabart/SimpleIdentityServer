using Microsoft.Data.Entity;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public static class AuthorizationCodeMapping
    {
        public static void AddAuthorizationCodeMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthorizationCode>()
                .ToTable("authorizationCodes")
                .HasKey(a => a.Code);
            /*
            ToTable("authorizationCodes");
            HasKey(a => a.Code);
            Property(a => a.RedirectUri);
            Property(a => a.CreateDateTime);
            Property(a => a.ClientId);
            Property(a => a.IdTokenPayload);
            Property(a => a.UserInfoPayLoad);
            Property(a => a.Scopes);
            */
        }
    }
}
