using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Metadata;

namespace SimpleIdentityServer.DataAccess.SqlServer.Migrations
{
    public partial class Initialize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "authorizationCodes",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(nullable: true),
                    CreateDateTime = table.Column<DateTime>(nullable: false),
                    IdTokenPayload = table.Column<string>(nullable: true),
                    RedirectUri = table.Column<string>(nullable: true),
                    Scopes = table.Column<string>(nullable: true),
                    UserInfoPayLoad = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationCode", x => x.Code);
                });
            migrationBuilder.CreateTable(
                name: "claims",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claim", x => x.Code);
                });
            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    ClientId = table.Column<string>(nullable: false),
                    ApplicationType = table.Column<int>(nullable: false),
                    ClientName = table.Column<string>(nullable: true),
                    ClientSecret = table.Column<string>(nullable: true),
                    ClientUri = table.Column<string>(nullable: true),
                    Contacts = table.Column<string>(nullable: true),
                    DefaultAcrValues = table.Column<string>(nullable: true),
                    DefaultMaxAge = table.Column<double>(nullable: false),
                    GrantTypes = table.Column<string>(nullable: true),
                    IdTokenEncryptedResponseAlg = table.Column<string>(nullable: true),
                    IdTokenEncryptedResponseEnc = table.Column<string>(nullable: true),
                    IdTokenSignedResponseAlg = table.Column<string>(nullable: true),
                    InitiateLoginUri = table.Column<string>(nullable: true),
                    JwksUri = table.Column<string>(nullable: true),
                    LogoUri = table.Column<string>(nullable: true),
                    PolicyUri = table.Column<string>(nullable: true),
                    RedirectionUrls = table.Column<string>(nullable: true),
                    RequestObjectEncryptionAlg = table.Column<string>(nullable: true),
                    RequestObjectEncryptionEnc = table.Column<string>(nullable: true),
                    RequestObjectSigningAlg = table.Column<string>(nullable: true),
                    RequestUris = table.Column<string>(nullable: true),
                    RequireAuthTime = table.Column<bool>(nullable: false),
                    ResponseTypes = table.Column<string>(nullable: true),
                    SectorIdentifierUri = table.Column<string>(nullable: true),
                    SubjectType = table.Column<string>(nullable: true),
                    TokenEndPointAuthMethod = table.Column<int>(nullable: false),
                    TokenEndPointAuthSigningAlg = table.Column<string>(nullable: true),
                    TosUri = table.Column<string>(nullable: true),
                    UserInfoEncryptedResponseAlg = table.Column<string>(nullable: true),
                    UserInfoEncryptedResponseEnc = table.Column<string>(nullable: true),
                    UserInfoSignedResponseAlg = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.ClientId);
                });
            migrationBuilder.CreateTable(
                name: "grantedTokens",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccessToken = table.Column<string>(nullable: true),
                    ClientId = table.Column<string>(nullable: true),
                    CreateDateTime = table.Column<DateTime>(nullable: false),
                    ExpiresIn = table.Column<int>(nullable: false),
                    IdTokenPayLoad = table.Column<string>(nullable: true),
                    RefreshToken = table.Column<string>(nullable: true),
                    Scope = table.Column<string>(nullable: true),
                    UserInfoPayLoad = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrantedToken", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "resourceOwners",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    BirthDate = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    EmailVerified = table.Column<bool>(nullable: false),
                    FamilyName = table.Column<string>(nullable: true),
                    Gender = table.Column<string>(nullable: true),
                    GivenName = table.Column<string>(nullable: true),
                    Locale = table.Column<string>(nullable: true),
                    MiddleName = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    NickName = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberVerified = table.Column<bool>(nullable: false),
                    Picture = table.Column<string>(nullable: true),
                    PreferredUserName = table.Column<string>(nullable: true),
                    Profile = table.Column<string>(nullable: true),
                    UpdatedAt = table.Column<double>(nullable: false),
                    WebSite = table.Column<string>(nullable: true),
                    ZoneInfo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceOwner", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "scopes",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    IsDisplayedInConsent = table.Column<bool>(nullable: false),
                    IsExposed = table.Column<bool>(nullable: false),
                    IsOpenIdScope = table.Column<bool>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scope", x => x.Name);
                });
            migrationBuilder.CreateTable(
                name: "translations",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    LanguageTag = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translation", x => new { x.Code, x.LanguageTag });
                });
            migrationBuilder.CreateTable(
                name: "jsonWebKeys",
                columns: table => new
                {
                    Kid = table.Column<string>(nullable: false),
                    Alg = table.Column<int>(nullable: false),
                    ClientId = table.Column<string>(nullable: true),
                    KeyOps = table.Column<string>(nullable: true),
                    Kty = table.Column<int>(nullable: false),
                    SerializedKey = table.Column<string>(nullable: true),
                    Use = table.Column<int>(nullable: false),
                    X5t = table.Column<string>(nullable: true),
                    X5tS256 = table.Column<string>(nullable: true),
                    X5u = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonWebKey", x => x.Kid);
                    table.ForeignKey(
                        name: "FK_JsonWebKey_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Country = table.Column<string>(nullable: true),
                    Formatted = table.Column<string>(nullable: true),
                    Locality = table.Column<string>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    Region = table.Column<string>(nullable: true),
                    ResourceOwnerId = table.Column<string>(nullable: true),
                    StreetAddress = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Address_ResourceOwner_ResourceOwnerId",
                        column: x => x.ResourceOwnerId,
                        principalTable: "resourceOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "consents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClientId = table.Column<string>(nullable: true),
                    ResourceOwnerId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consent_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Consent_ResourceOwner_ResourceOwnerId",
                        column: x => x.ResourceOwnerId,
                        principalTable: "resourceOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "clientScopes",
                columns: table => new
                {
                    ClientId = table.Column<string>(nullable: false),
                    ScopeName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientScope", x => new { x.ClientId, x.ScopeName });
                    table.ForeignKey(
                        name: "FK_ClientScope_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientScope_Scope_ScopeName",
                        column: x => x.ScopeName,
                        principalTable: "scopes",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "scopeClaims",
                columns: table => new
                {
                    ClaimCode = table.Column<string>(nullable: false),
                    ScopeName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScopeClaim", x => new { x.ClaimCode, x.ScopeName });
                    table.ForeignKey(
                        name: "FK_ScopeClaim_Claim_ClaimCode",
                        column: x => x.ClaimCode,
                        principalTable: "claims",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScopeClaim_Scope_ScopeName",
                        column: x => x.ScopeName,
                        principalTable: "scopes",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "consentClaims",
                columns: table => new
                {
                    ConsentId = table.Column<int>(nullable: false),
                    ClaimCode = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentClaim", x => new { x.ConsentId, x.ClaimCode });
                    table.ForeignKey(
                        name: "FK_ConsentClaim_Claim_ClaimCode",
                        column: x => x.ClaimCode,
                        principalTable: "claims",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentClaim_Consent_ConsentId",
                        column: x => x.ConsentId,
                        principalTable: "consents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "consentScopes",
                columns: table => new
                {
                    ConsentId = table.Column<int>(nullable: false),
                    ScopeName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentScope", x => new { x.ConsentId, x.ScopeName });
                    table.ForeignKey(
                        name: "FK_ConsentScope_Consent_ConsentId",
                        column: x => x.ConsentId,
                        principalTable: "consents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentScope_Scope_ScopeName",
                        column: x => x.ScopeName,
                        principalTable: "scopes",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("addresses");
            migrationBuilder.DropTable("authorizationCodes");
            migrationBuilder.DropTable("clientScopes");
            migrationBuilder.DropTable("consentClaims");
            migrationBuilder.DropTable("consentScopes");
            migrationBuilder.DropTable("grantedTokens");
            migrationBuilder.DropTable("jsonWebKeys");
            migrationBuilder.DropTable("scopeClaims");
            migrationBuilder.DropTable("translations");
            migrationBuilder.DropTable("consents");
            migrationBuilder.DropTable("claims");
            migrationBuilder.DropTable("scopes");
            migrationBuilder.DropTable("clients");
            migrationBuilder.DropTable("resourceOwners");
        }
    }
}
