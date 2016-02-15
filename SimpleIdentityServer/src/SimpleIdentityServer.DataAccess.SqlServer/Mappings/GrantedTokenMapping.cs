using SimpleIdentityServer.DataAccess.SqlServer.Models;
using System.Data.Entity.ModelConfiguration;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public sealed class GrantedTokenMapping : EntityTypeConfiguration<GrantedToken>
    {
        public GrantedTokenMapping()
        {
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
        }
    }
}
