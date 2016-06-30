using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SimpleIdentityServer.DataAccess.SqlServer;

namespace SimpleIdentityServer.DataAccess.SqlServer.Migrations
{
    [DbContext(typeof(SimpleIdentityServerContext))]
    partial class SimpleIdentityServerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rc2-20901")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.Address", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Country");

                    b.Property<string>("Formatted");

                    b.Property<string>("Locality");

                    b.Property<string>("PostalCode");

                    b.Property<string>("Region");

                    b.Property<string>("ResourceOwnerForeignKey");

                    b.Property<string>("StreetAddress");

                    b.HasKey("Id");

                    b.HasIndex("ResourceOwnerForeignKey");

                    b.ToTable("addresses");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.AuthorizationCode", b =>
                {
                    b.Property<string>("Code");

                    b.Property<string>("ClientId");

                    b.Property<DateTime>("CreateDateTime");

                    b.Property<string>("IdTokenPayload");

                    b.Property<string>("RedirectUri");

                    b.Property<string>("Scopes");

                    b.Property<string>("UserInfoPayLoad");

                    b.HasKey("Code");

                    b.ToTable("authorizationCodes");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.Claim", b =>
                {
                    b.Property<string>("Code");

                    b.HasKey("Code");

                    b.ToTable("claims");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.Client", b =>
                {
                    b.Property<string>("ClientId");

                    b.Property<int>("ApplicationType");

                    b.Property<string>("ClientName");

                    b.Property<string>("ClientSecret");

                    b.Property<string>("ClientUri");

                    b.Property<string>("Contacts");

                    b.Property<string>("DefaultAcrValues");

                    b.Property<double>("DefaultMaxAge");

                    b.Property<string>("GrantTypes");

                    b.Property<string>("IdTokenEncryptedResponseAlg");

                    b.Property<string>("IdTokenEncryptedResponseEnc");

                    b.Property<string>("IdTokenSignedResponseAlg");

                    b.Property<string>("InitiateLoginUri");

                    b.Property<string>("JwksUri");

                    b.Property<string>("LogoUri");

                    b.Property<string>("PolicyUri");

                    b.Property<string>("RedirectionUrls");

                    b.Property<string>("RequestObjectEncryptionAlg");

                    b.Property<string>("RequestObjectEncryptionEnc");

                    b.Property<string>("RequestObjectSigningAlg");

                    b.Property<string>("RequestUris");

                    b.Property<bool>("RequireAuthTime");

                    b.Property<string>("ResponseTypes");

                    b.Property<string>("SectorIdentifierUri");

                    b.Property<string>("SubjectType");

                    b.Property<int>("TokenEndPointAuthMethod");

                    b.Property<string>("TokenEndPointAuthSigningAlg");

                    b.Property<string>("TosUri");

                    b.Property<string>("UserInfoEncryptedResponseAlg");

                    b.Property<string>("UserInfoEncryptedResponseEnc");

                    b.Property<string>("UserInfoSignedResponseAlg");

                    b.HasKey("ClientId");

                    b.ToTable("clients");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ClientScope", b =>
                {
                    b.Property<string>("ClientId");

                    b.Property<string>("ScopeName");

                    b.HasKey("ClientId", "ScopeName");

                    b.HasIndex("ClientId");

                    b.HasIndex("ScopeName");

                    b.ToTable("clientScopes");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.Consent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClientId");

                    b.Property<string>("ResourceOwnerId");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.HasIndex("ResourceOwnerId");

                    b.ToTable("consents");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ConsentClaim", b =>
                {
                    b.Property<int>("ConsentId");

                    b.Property<string>("ClaimCode");

                    b.HasKey("ConsentId", "ClaimCode");

                    b.HasIndex("ClaimCode");

                    b.HasIndex("ConsentId");

                    b.ToTable("consentClaims");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ConsentScope", b =>
                {
                    b.Property<int>("ConsentId");

                    b.Property<string>("ScopeName");

                    b.HasKey("ConsentId", "ScopeName");

                    b.HasIndex("ConsentId");

                    b.HasIndex("ScopeName");

                    b.ToTable("consentScopes");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.GrantedToken", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("AccessToken");

                    b.Property<string>("ClientId");

                    b.Property<DateTime>("CreateDateTime");

                    b.Property<int>("ExpiresIn");

                    b.Property<string>("IdTokenPayLoad");

                    b.Property<string>("ParentRefreshToken");

                    b.Property<string>("RefreshToken");

                    b.Property<string>("Scope");

                    b.Property<string>("UserInfoPayLoad");

                    b.HasKey("Id");

                    b.ToTable("grantedTokens");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.JsonWebKey", b =>
                {
                    b.Property<string>("Kid");

                    b.Property<int>("Alg");

                    b.Property<string>("ClientId");

                    b.Property<string>("KeyOps");

                    b.Property<int>("Kty");

                    b.Property<string>("SerializedKey");

                    b.Property<int>("Use");

                    b.Property<string>("X5t");

                    b.Property<string>("X5tS256");

                    b.Property<string>("X5u");

                    b.HasKey("Kid");

                    b.HasIndex("ClientId");

                    b.ToTable("jsonWebKeys");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ResourceOwner", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("BirthDate");

                    b.Property<string>("Email");

                    b.Property<bool>("EmailVerified");

                    b.Property<string>("FamilyName");

                    b.Property<string>("Gender");

                    b.Property<string>("GivenName");

                    b.Property<bool>("IsLocalAccount");

                    b.Property<string>("Locale");

                    b.Property<string>("MiddleName");

                    b.Property<string>("Name");

                    b.Property<string>("NickName");

                    b.Property<string>("Password");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberVerified");

                    b.Property<string>("Picture");

                    b.Property<string>("PreferredUserName");

                    b.Property<string>("Profile");

                    b.Property<double>("UpdatedAt");

                    b.Property<string>("WebSite");

                    b.Property<string>("ZoneInfo");

                    b.HasKey("Id");

                    b.ToTable("resourceOwners");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ResourceOwnerRole", b =>
                {
                    b.Property<string>("ResourceOwnerId");

                    b.Property<string>("RoleName");

                    b.HasKey("ResourceOwnerId", "RoleName");

                    b.HasIndex("ResourceOwnerId");

                    b.HasIndex("RoleName");

                    b.ToTable("resourceOwnerRoles");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.Role", b =>
                {
                    b.Property<string>("Name");

                    b.Property<string>("Description");

                    b.HasKey("Name");

                    b.ToTable("roles");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.Scope", b =>
                {
                    b.Property<string>("Name");

                    b.Property<string>("Description")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<bool>("IsDisplayedInConsent");

                    b.Property<bool>("IsExposed");

                    b.Property<bool>("IsOpenIdScope");

                    b.Property<int>("Type");

                    b.HasKey("Name");

                    b.ToTable("scopes");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ScopeClaim", b =>
                {
                    b.Property<string>("ClaimCode");

                    b.Property<string>("ScopeName");

                    b.HasKey("ClaimCode", "ScopeName");

                    b.HasIndex("ClaimCode");

                    b.HasIndex("ScopeName");

                    b.ToTable("scopeClaims");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.Translation", b =>
                {
                    b.Property<string>("Code")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("LanguageTag");

                    b.Property<string>("Value");

                    b.HasKey("Code", "LanguageTag");

                    b.ToTable("translations");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.Address", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.ResourceOwner")
                        .WithOne()
                        .HasForeignKey("SimpleIdentityServer.DataAccess.SqlServer.Models.Address", "ResourceOwnerForeignKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ClientScope", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Client")
                        .WithMany()
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Scope")
                        .WithMany()
                        .HasForeignKey("ScopeName")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.Consent", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Client")
                        .WithMany()
                        .HasForeignKey("ClientId");

                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.ResourceOwner")
                        .WithMany()
                        .HasForeignKey("ResourceOwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ConsentClaim", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Claim")
                        .WithMany()
                        .HasForeignKey("ClaimCode")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Consent")
                        .WithMany()
                        .HasForeignKey("ConsentId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ConsentScope", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Consent")
                        .WithMany()
                        .HasForeignKey("ConsentId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Scope")
                        .WithMany()
                        .HasForeignKey("ScopeName")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.JsonWebKey", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Client")
                        .WithMany()
                        .HasForeignKey("ClientId");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ResourceOwnerRole", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.ResourceOwner")
                        .WithMany()
                        .HasForeignKey("ResourceOwnerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Role")
                        .WithMany()
                        .HasForeignKey("RoleName")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ScopeClaim", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Claim")
                        .WithMany()
                        .HasForeignKey("ClaimCode")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Scope")
                        .WithMany()
                        .HasForeignKey("ScopeName")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
