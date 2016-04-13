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
                    table.PrimaryKey("PK_ResourceSet", x => x.Id);
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
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(nullable: true),
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Scopes");
            migrationBuilder.DropTable("Tickets");
            migrationBuilder.DropTable("ResourceSets");
        }
    }
}
