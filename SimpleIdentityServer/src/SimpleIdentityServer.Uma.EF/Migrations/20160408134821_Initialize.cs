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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("ResourceSets");
        }
    }
}
