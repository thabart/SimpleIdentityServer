using SimpleIdentityServer.DataAccess.SqlServer.Models;
using System.Data.Entity.ModelConfiguration;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public sealed class ClientMapping : EntityTypeConfiguration<Client>
    {
        public ClientMapping()
        {
            ToTable("clients");
            HasKey(c => c.ClientId);
            Property(c => c.ClientSecret);
            Property(c => c.ClientName);
            Property(c => c.LogoUri);
            Property(c => c.ClientUri);
            Property(c => c.PolicyUri);
            Property(c => c.TosUri);
            Property(c => c.IdTokenSignedResponseAlg);
            Property(c => c.IdTokenEncryptedResponseAlg);
            Property(c => c.IdTokenEncryptedResponseEnc);
            Property(c => c.TokenEndPointAuthMethod);
            Property(c => c.ResponseTypes);
            Property(c => c.GrantTypes);
            Property(c => c.RedirectionUrls);
            Property(c => c.ApplicationType);
            Property(c => c.JwksUri);
            Property(c => c.Contacts);
            Property(c => c.SectorIdentifierUri);
            Property(c => c.SubjectType);
            Property(c => c.UserInfoSignedResponseAlg);
            Property(c => c.UserInfoEncryptedResponseAlg);
            Property(c => c.UserInfoEncryptedResponseEnc);
            Property(c => c.RequestObjectSigningAlg);
            Property(c => c.RequestObjectEncryptionAlg);
            Property(c => c.RequestObjectEncryptionEnc);
            Property(c => c.TokenEndPointAuthSigningAlg);
            Property(c => c.DefaultMaxAge);
            Property(c => c.RequireAuthTime);
            Property(c => c.DefaultAcrValues);
            Property(c => c.InitiateLoginUri);
            Property(c => c.RequestUris);
            // Set AllowedScopes & JsonWebKeys
            HasMany(c => c.AllowedScopes)
                .WithMany(c => c.Clients)
                .Map(c =>
                {
                    c.MapLeftKey("ClientId");
                    c.MapRightKey("ScopeName");
                    c.ToTable("clientScopes");
                });
            HasMany(c => c.JsonWebKeys);
        }
    }
}
