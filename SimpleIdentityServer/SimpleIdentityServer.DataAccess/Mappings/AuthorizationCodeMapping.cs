using System.Data.Entity.ModelConfiguration;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public sealed class AuthorizationCodeMapping : EntityTypeConfiguration<AuthorizationCode>
    {
        public AuthorizationCodeMapping()
        {
            ToTable("authorizationCodes");
            HasKey(a => a.Code);
            Property(a => a.RedirectUri);
            Property(a => a.CreateDateTime);
            Property(a => a.ClientId);
            Property(a => a.IdTokenPayload);
            Property(a => a.UserInfoPayLoad);
            Property(a => a.Scopes);
        }
    }
}
