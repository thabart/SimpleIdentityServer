using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

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
                    table.PrimaryKey("PK_authorizationCodes", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "claims",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claims", x => x.Code);
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
                    table.PrimaryKey("PK_clients", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "grantedTokens",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AccessToken = table.Column<string>(nullable: true),
                    ClientId = table.Column<string>(nullable: true),
                    CreateDateTime = table.Column<DateTime>(nullable: false),
                    ExpiresIn = table.Column<int>(nullable: false),
                    IdTokenPayLoad = table.Column<string>(nullable: true),
                    ParentRefreshToken = table.Column<string>(nullable: true),
                    RefreshToken = table.Column<string>(nullable: true),
                    Scope = table.Column<string>(nullable: true),
                    UserInfoPayLoad = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grantedTokens", x => x.Id);
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
                    IsLocalAccount = table.Column<bool>(nullable: false),
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
                    table.PrimaryKey("PK_resourceOwners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "scopes",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: true),
                    IsDisplayedInConsent = table.Column<bool>(nullable: false),
                    IsExposed = table.Column<bool>(nullable: false),
                    IsOpenIdScope = table.Column<bool>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scopes", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "translations",
                columns: table => new
                {
                    Code = table.Column<string>(maxLength: 255, nullable: false),
                    LanguageTag = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_translations", x => new { x.Code, x.LanguageTag });
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
                    table.PrimaryKey("PK_jsonWebKeys", x => x.Kid);
                    table.ForeignKey(
                        name: "FK_jsonWebKeys_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Country = table.Column<string>(nullable: true),
                    Formatted = table.Column<string>(nullable: true),
                    Locality = table.Column<string>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    Region = table.Column<string>(nullable: true),
                    ResourceOwnerForeignKey = table.Column<string>(nullable: true),
                    StreetAddress = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_addresses_resourceOwners_ResourceOwnerForeignKey",
                        column: x => x.ResourceOwnerForeignKey,
                        principalTable: "resourceOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.PrimaryKey("PK_consents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_consents_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_consents_resourceOwners_ResourceOwnerId",
                        column: x => x.ResourceOwnerId,
                        principalTable: "resourceOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resourceOwnerRoles",
                columns: table => new
                {
                    ResourceOwnerId = table.Column<string>(nullable: false),
                    RoleName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resourceOwnerRoles", x => new { x.ResourceOwnerId, x.RoleName });
                    table.ForeignKey(
                        name: "FK_resourceOwnerRoles_resourceOwners_ResourceOwnerId",
                        column: x => x.ResourceOwnerId,
                        principalTable: "resourceOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resourceOwnerRoles_roles_RoleName",
                        column: x => x.RoleName,
                        principalTable: "roles",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
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
                    table.PrimaryKey("PK_clientScopes", x => new { x.ClientId, x.ScopeName });
                    table.ForeignKey(
                        name: "FK_clientScopes_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_clientScopes_scopes_ScopeName",
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
                    table.PrimaryKey("PK_scopeClaims", x => new { x.ClaimCode, x.ScopeName });
                    table.ForeignKey(
                        name: "FK_scopeClaims_claims_ClaimCode",
                        column: x => x.ClaimCode,
                        principalTable: "claims",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_scopeClaims_scopes_ScopeName",
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
                    table.PrimaryKey("PK_consentClaims", x => new { x.ConsentId, x.ClaimCode });
                    table.ForeignKey(
                        name: "FK_consentClaims_claims_ClaimCode",
                        column: x => x.ClaimCode,
                        principalTable: "claims",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_consentClaims_consents_ConsentId",
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
                    table.PrimaryKey("PK_consentScopes", x => new { x.ConsentId, x.ScopeName });
                    table.ForeignKey(
                        name: "FK_consentScopes_consents_ConsentId",
                        column: x => x.ConsentId,
                        principalTable: "consents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_consentScopes_scopes_ScopeName",
                        column: x => x.ScopeName,
                        principalTable: "scopes",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_addresses_ResourceOwnerForeignKey",
                table: "addresses",
                column: "ResourceOwnerForeignKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clientScopes_ClientId",
                table: "clientScopes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_clientScopes_ScopeName",
                table: "clientScopes",
                column: "ScopeName");

            migrationBuilder.CreateIndex(
                name: "IX_consents_ClientId",
                table: "consents",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_consents_ResourceOwnerId",
                table: "consents",
                column: "ResourceOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_consentClaims_ClaimCode",
                table: "consentClaims",
                column: "ClaimCode");

            migrationBuilder.CreateIndex(
                name: "IX_consentClaims_ConsentId",
                table: "consentClaims",
                column: "ConsentId");

            migrationBuilder.CreateIndex(
                name: "IX_consentScopes_ConsentId",
                table: "consentScopes",
                column: "ConsentId");

            migrationBuilder.CreateIndex(
                name: "IX_consentScopes_ScopeName",
                table: "consentScopes",
                column: "ScopeName");

            migrationBuilder.CreateIndex(
                name: "IX_jsonWebKeys_ClientId",
                table: "jsonWebKeys",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_resourceOwnerRoles_ResourceOwnerId",
                table: "resourceOwnerRoles",
                column: "ResourceOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_resourceOwnerRoles_RoleName",
                table: "resourceOwnerRoles",
                column: "RoleName");

            migrationBuilder.CreateIndex(
                name: "IX_scopeClaims_ClaimCode",
                table: "scopeClaims",
                column: "ClaimCode");

            migrationBuilder.CreateIndex(
                name: "IX_scopeClaims_ScopeName",
                table: "scopeClaims",
                column: "ScopeName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "authorizationCodes");

            migrationBuilder.DropTable(
                name: "clientScopes");

            migrationBuilder.DropTable(
                name: "consentClaims");

            migrationBuilder.DropTable(
                name: "consentScopes");

            migrationBuilder.DropTable(
                name: "grantedTokens");

            migrationBuilder.DropTable(
                name: "jsonWebKeys");

            migrationBuilder.DropTable(
                name: "resourceOwnerRoles");

            migrationBuilder.DropTable(
                name: "scopeClaims");

            migrationBuilder.DropTable(
                name: "translations");

            migrationBuilder.DropTable(
                name: "consents");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "claims");

            migrationBuilder.DropTable(
                name: "scopes");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "resourceOwners");
        }
    }
}
