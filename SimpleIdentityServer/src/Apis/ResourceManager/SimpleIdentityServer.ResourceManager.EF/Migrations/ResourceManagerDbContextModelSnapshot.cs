using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SimpleIdentityServer.ResourceManager.EF;

namespace SimpleIdentityServer.ResourceManager.EF.Migrations
{
    [DbContext(typeof(ResourceManagerDbContext))]
    partial class ResourceManagerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.3")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SimpleIdentityServer.ResourceManager.EF.Models.Asset", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Name");

                    b.Property<string>("ResourceId");

                    b.HasKey("Id");

                    b.ToTable("assets");
                });
        }
    }
}
