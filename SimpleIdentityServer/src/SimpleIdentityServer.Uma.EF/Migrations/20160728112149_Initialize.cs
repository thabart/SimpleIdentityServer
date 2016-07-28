using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SimpleIdentityServer.Uma.EF.Migrations
{
    public partial class Initialize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResourceSets",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    IconUri = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Scopes = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Uri = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scopes",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    IconUri = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PolicyRules",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Claims = table.Column<string>(nullable: true),
                    ClientIdsAllowed = table.Column<string>(nullable: true),
                    IsResourceOwnerConsentNeeded = table.Column<bool>(nullable: false),
                    PolicyId = table.Column<string>(nullable: true),
                    Scopes = table.Column<string>(nullable: true),
                    Script = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyRules_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PolicyResource",
                columns: table => new
                {
                    PolicyId = table.Column<string>(nullable: false),
                    ResourceSetId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyResource", x => new { x.PolicyId, x.ResourceSetId });
                    table.ForeignKey(
                        name: "FK_PolicyResource_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PolicyResource_ResourceSets_ResourceSetId",
                        column: x => x.ResourceSetId,
                        principalTable: "ResourceSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(nullable: true),
                    CreateDateTime = table.Column<DateTime>(nullable: false),
                    ExpirationDateTime = table.Column<DateTime>(nullable: false),
                    ResourceSetId = table.Column<string>(nullable: true),
                    Scopes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_ResourceSets_ResourceSetId",
                        column: x => x.ResourceSetId,
                        principalTable: "ResourceSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rpts",
                columns: table => new
                {
                    Value = table.Column<string>(nullable: false),
                    CreateDateTime = table.Column<DateTime>(nullable: false),
                    ExpirationDateTime = table.Column<DateTime>(nullable: false),
                    ResourceSetId = table.Column<string>(nullable: true),
                    TicketId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rpts", x => x.Value);
                    table.ForeignKey(
                        name: "FK_Rpts_ResourceSets_ResourceSetId",
                        column: x => x.ResourceSetId,
                        principalTable: "ResourceSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rpts_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyResource_PolicyId",
                table: "PolicyResource",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyResource_ResourceSetId",
                table: "PolicyResource",
                column: "ResourceSetId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRules_PolicyId",
                table: "PolicyRules",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Rpts_ResourceSetId",
                table: "Rpts",
                column: "ResourceSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Rpts_TicketId",
                table: "Rpts",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ResourceSetId",
                table: "Tickets",
                column: "ResourceSetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolicyResource");

            migrationBuilder.DropTable(
                name: "PolicyRules");

            migrationBuilder.DropTable(
                name: "Rpts");

            migrationBuilder.DropTable(
                name: "Scopes");

            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "ResourceSets");
        }
    }
}
