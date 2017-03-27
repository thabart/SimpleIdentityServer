using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SimpleIdentityServer.DataAccess.SqlServer;

namespace SimpleIdentityServer.DataAccess.SqlServer.Migrations
{
    [DbContext(typeof(SimpleIdentityServerContext))]
    [Migration("20170327143147_Initialize")]
    partial class Initialize
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.AuthorizationCode", b =>
                {
                    b.Property<string>("Code");

                    b.Property<string>("ClientId");

                    b.Property<string>("CodeChallenge");

                    b.Property<int?>("CodeChallengeMethod");

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

                    b.Property<bool>("IsIdentifier");

                    b.Property<string>("Type");

                    b.HasKey("Code");

                    b.ToTable("claims");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.Client", b =>
                {
                    b.Property<string>("ClientId");

                    b.Property<int>("ApplicationType");

                    b.Property<string>("ClientName");

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

                    b.Property<bool>("RequirePkce");

                    b.Property<string>("ResponseTypes");

                    b.Property<bool>("ScimProfile");

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

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ClientSecret", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ClientId");

                    b.Property<int>("Type");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("ClientSecrets");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ConfirmationCode", b =>
                {
                    b.Property<string>("Code");

                    b.Property<DateTime>("CreateDateTime");

                    b.Property<int>("ExpiresIn");

                    b.Property<bool>("IsConfirmed");

                    b.HasKey("Code");

                    b.ToTable("confirmationCodes");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.Consent", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ClientId");

                    b.Property<string>("ResourceOwnerId");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.HasIndex("ResourceOwnerId");

                    b.ToTable("consents");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ConsentClaim", b =>
                {
                    b.Property<string>("ConsentId");

                    b.Property<string>("ClaimCode");

                    b.HasKey("ConsentId", "ClaimCode");

                    b.HasIndex("ClaimCode");

                    b.HasIndex("ConsentId");

                    b.ToTable("consentClaims");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ConsentScope", b =>
                {
                    b.Property<string>("ConsentId");

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

                    b.Property<string>("ParentTokenId");

                    b.Property<string>("RefreshToken");

                    b.Property<string>("Scope");

                    b.Property<string>("UserInfoPayLoad");

                    b.HasKey("Id");

                    b.HasIndex("ParentTokenId");

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

                    b.Property<bool>("IsLocalAccount");

                    b.Property<string>("Password");

                    b.Property<int>("TwoFactorAuthentication");

                    b.HasKey("Id");

                    b.ToTable("resourceOwners");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ResourceOwnerClaim", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ClaimCode");

                    b.Property<string>("ResourceOwnerId");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("ClaimCode");

                    b.HasIndex("ResourceOwnerId");

                    b.ToTable("resourceOwnerClaims");
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

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ClientScope", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Client", "Client")
                        .WithMany("ClientScopes")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Scope", "Scope")
                        .WithMany("ClientScopes")
                        .HasForeignKey("ScopeName")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ClientSecret", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Client", "Client")
                        .WithMany("ClientSecrets")
                        .HasForeignKey("ClientId");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.Consent", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Client", "Client")
                        .WithMany("Consents")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.ResourceOwner", "ResourceOwner")
                        .WithMany("Consents")
                        .HasForeignKey("ResourceOwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ConsentClaim", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Claim", "Claim")
                        .WithMany("ConsentClaims")
                        .HasForeignKey("ClaimCode")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Consent", "Consent")
                        .WithMany("ConsentClaims")
                        .HasForeignKey("ConsentId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ConsentScope", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Consent", "Consent")
                        .WithMany("ConsentScopes")
                        .HasForeignKey("ConsentId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Scope", "Scope")
                        .WithMany("ConsentScopes")
                        .HasForeignKey("ScopeName")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.GrantedToken", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.GrantedToken", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentTokenId");
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.JsonWebKey", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Client", "Client")
                        .WithMany("JsonWebKeys")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ResourceOwnerClaim", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Claim", "Claim")
                        .WithMany("ResourceOwnerClaims")
                        .HasForeignKey("ClaimCode")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.ResourceOwner", "ResourceOwner")
                        .WithMany("Claims")
                        .HasForeignKey("ResourceOwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.DataAccess.SqlServer.Models.ScopeClaim", b =>
                {
                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Claim", "Claim")
                        .WithMany("ScopeClaims")
                        .HasForeignKey("ClaimCode")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SimpleIdentityServer.DataAccess.SqlServer.Models.Scope", "Scope")
                        .WithMany("ScopeClaims")
                        .HasForeignKey("ScopeName")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
