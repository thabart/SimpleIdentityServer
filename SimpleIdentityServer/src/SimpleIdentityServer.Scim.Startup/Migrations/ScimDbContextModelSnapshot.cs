using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SimpleIdentityServer.Scim.Db.EF;

namespace SimpleIdentityServer.Scim.Startup.Migrations
{
    [DbContext(typeof(ScimDbContext))]
    partial class ScimDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SimpleIdentityServer.Scim.Db.EF.Models.MetaData", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Location");

                    b.Property<string>("ResourceType");

                    b.Property<string>("SchemaId");

                    b.HasKey("Id");

                    b.HasIndex("SchemaId")
                        .IsUnique();

                    b.ToTable("metaData");
                });

            modelBuilder.Entity("SimpleIdentityServer.Scim.Db.EF.Models.Representation", b =>
                {
                    b.Property<string>("Id");

                    b.Property<DateTime>("Created");

                    b.Property<DateTime>("LastModified");

                    b.Property<string>("ResourceType");

                    b.Property<string>("Version");

                    b.HasKey("Id");

                    b.ToTable("representations");
                });

            modelBuilder.Entity("SimpleIdentityServer.Scim.Db.EF.Models.RepresentationAttribute", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("RepresentationAttributeIdParent");

                    b.Property<string>("RepresentationId");

                    b.Property<string>("SchemaAttributeId");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("RepresentationAttributeIdParent");

                    b.HasIndex("RepresentationId");

                    b.HasIndex("SchemaAttributeId");

                    b.ToTable("representationAttributes");
                });

            modelBuilder.Entity("SimpleIdentityServer.Scim.Db.EF.Models.Schema", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("schemas");
                });

            modelBuilder.Entity("SimpleIdentityServer.Scim.Db.EF.Models.SchemaAttribute", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("CanonicalValues");

                    b.Property<bool>("CaseExact");

                    b.Property<string>("Description");

                    b.Property<bool>("IsCommon");

                    b.Property<bool>("MultiValued");

                    b.Property<string>("Mutability");

                    b.Property<string>("Name");

                    b.Property<string>("ReferenceTypes");

                    b.Property<bool>("Required");

                    b.Property<string>("Returned");

                    b.Property<string>("SchemaAttributeIdParent");

                    b.Property<string>("SchemaId");

                    b.Property<string>("Type");

                    b.Property<string>("Uniqueness");

                    b.HasKey("Id");

                    b.HasIndex("SchemaAttributeIdParent");

                    b.HasIndex("SchemaId");

                    b.ToTable("schemaAttributes");
                });

            modelBuilder.Entity("SimpleIdentityServer.Scim.Db.EF.Models.MetaData", b =>
                {
                    b.HasOne("SimpleIdentityServer.Scim.Db.EF.Models.Schema", "Schema")
                        .WithOne("Meta")
                        .HasForeignKey("SimpleIdentityServer.Scim.Db.EF.Models.MetaData", "SchemaId");
                });

            modelBuilder.Entity("SimpleIdentityServer.Scim.Db.EF.Models.RepresentationAttribute", b =>
                {
                    b.HasOne("SimpleIdentityServer.Scim.Db.EF.Models.RepresentationAttribute", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("RepresentationAttributeIdParent");

                    b.HasOne("SimpleIdentityServer.Scim.Db.EF.Models.Representation", "Representation")
                        .WithMany("Attributes")
                        .HasForeignKey("RepresentationId");

                    b.HasOne("SimpleIdentityServer.Scim.Db.EF.Models.SchemaAttribute", "SchemaAttribute")
                        .WithMany("RepresentationAttributes")
                        .HasForeignKey("SchemaAttributeId");
                });

            modelBuilder.Entity("SimpleIdentityServer.Scim.Db.EF.Models.SchemaAttribute", b =>
                {
                    b.HasOne("SimpleIdentityServer.Scim.Db.EF.Models.SchemaAttribute", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("SchemaAttributeIdParent");

                    b.HasOne("SimpleIdentityServer.Scim.Db.EF.Models.Schema", "Schema")
                        .WithMany("Attributes")
                        .HasForeignKey("SchemaId");
                });
        }
    }
}
