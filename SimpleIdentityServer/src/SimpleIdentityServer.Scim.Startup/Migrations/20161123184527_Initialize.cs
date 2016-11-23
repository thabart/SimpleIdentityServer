using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SimpleIdentityServer.Scim.Startup.Migrations
{
    public partial class Initialize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "representations",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: false),
                    ResourceType = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_representations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "schemas",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schemas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "metaData",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    ResourceType = table.Column<string>(nullable: true),
                    SchemaId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metaData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_metaData_schemas_SchemaId",
                        column: x => x.SchemaId,
                        principalTable: "schemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "schemaAttributes",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    CanonicalValues = table.Column<string>(nullable: true),
                    CaseExact = table.Column<bool>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    IsCommon = table.Column<bool>(nullable: false),
                    MultiValued = table.Column<bool>(nullable: false),
                    Mutability = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ReferenceTypes = table.Column<string>(nullable: true),
                    Required = table.Column<bool>(nullable: false),
                    Returned = table.Column<string>(nullable: true),
                    SchemaAttributeIdParent = table.Column<string>(nullable: true),
                    SchemaId = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Uniqueness = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schemaAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_schemaAttributes_schemaAttributes_SchemaAttributeIdParent",
                        column: x => x.SchemaAttributeIdParent,
                        principalTable: "schemaAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_schemaAttributes_schemas_SchemaId",
                        column: x => x.SchemaId,
                        principalTable: "schemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "representationAttributes",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    RepresentationAttributeIdParent = table.Column<string>(nullable: true),
                    RepresentationId = table.Column<string>(nullable: true),
                    SchemaAttributeId = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_representationAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_representationAttributes_representationAttributes_RepresentationAttributeIdParent",
                        column: x => x.RepresentationAttributeIdParent,
                        principalTable: "representationAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_representationAttributes_representations_RepresentationId",
                        column: x => x.RepresentationId,
                        principalTable: "representations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_representationAttributes_schemaAttributes_SchemaAttributeId",
                        column: x => x.SchemaAttributeId,
                        principalTable: "schemaAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_metaData_SchemaId",
                table: "metaData",
                column: "SchemaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_representationAttributes_RepresentationAttributeIdParent",
                table: "representationAttributes",
                column: "RepresentationAttributeIdParent");

            migrationBuilder.CreateIndex(
                name: "IX_representationAttributes_RepresentationId",
                table: "representationAttributes",
                column: "RepresentationId");

            migrationBuilder.CreateIndex(
                name: "IX_representationAttributes_SchemaAttributeId",
                table: "representationAttributes",
                column: "SchemaAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_schemaAttributes_SchemaAttributeIdParent",
                table: "schemaAttributes",
                column: "SchemaAttributeIdParent");

            migrationBuilder.CreateIndex(
                name: "IX_schemaAttributes_SchemaId",
                table: "schemaAttributes",
                column: "SchemaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "metaData");

            migrationBuilder.DropTable(
                name: "representationAttributes");

            migrationBuilder.DropTable(
                name: "representations");

            migrationBuilder.DropTable(
                name: "schemaAttributes");

            migrationBuilder.DropTable(
                name: "schemas");
        }
    }
}
