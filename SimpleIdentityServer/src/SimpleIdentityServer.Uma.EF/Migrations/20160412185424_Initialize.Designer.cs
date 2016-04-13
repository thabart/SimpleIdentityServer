using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using SimpleIdentityServer.Uma.EF;

namespace SimpleIdentityServer.Uma.EF.Migrations
{
    [DbContext(typeof(SimpleIdServerUmaContext))]
    [Migration("20160412185424_Initialize")]
    partial class Initialize
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.ResourceSet", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("IconUri");

                    b.Property<string>("Name");

                    b.Property<string>("Scopes");

                    b.Property<string>("Type");

                    b.Property<string>("Uri");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "ResourceSets");
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.Scope", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("IconUri");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Scopes");
                });
        }
    }
}
