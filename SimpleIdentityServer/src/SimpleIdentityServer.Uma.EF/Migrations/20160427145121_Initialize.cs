using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

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
                    Id = table.Column<string>(nullable: false),
                    ClientIdsAllowed = table.Column<string>(nullable: true),
                    IsCustom = table.Column<bool>(nullable: false),
                    IsResourceOwnerConsentNeeded = table.Column<bool>(nullable: false),
                    Scopes = table.Column<string>(nullable: true),
                    Script = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policy", x => x.Id);
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
                    table.PrimaryKey("PK_Scope", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "ResourceSets",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    IconUri = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    PolicyId = table.Column<string>(nullable: true),
                    Scopes = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Uri = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceSet_Policy_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.PrimaryKey("PK_Ticket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ticket_ResourceSet_ResourceSetId",
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
                    table.PrimaryKey("PK_Rpt", x => x.Value);
                    table.ForeignKey(
                        name: "FK_Rpt_ResourceSet_ResourceSetId",
                        column: x => x.ResourceSetId,
                        principalTable: "ResourceSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rpt_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Rpts");
            migrationBuilder.DropTable("Scopes");
            migrationBuilder.DropTable("Tickets");
            migrationBuilder.DropTable("ResourceSets");
            migrationBuilder.DropTable("Policies");
        }
    }
}
