using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentityServer4.Startup.Migrations.UserDb
{
    public partial class InitialIdentityServerMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Subject = table.Column<string>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    IsLocalAccount = table.Column<bool>(nullable: false),
                    Password = table.Column<string>(nullable: true),
                    Provider = table.Column<string>(nullable: true),
                    ProviderId = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Subject);
                });

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    UserSubject = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claims_Users_UserSubject",
                        column: x => x.UserSubject,
                        principalTable: "Users",
                        principalColumn: "Subject",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_UserSubject",
                table: "Claims",
                column: "UserSubject");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
