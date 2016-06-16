using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SimpleIdentityServer.Configuration.EF;

namespace SimpleIdentityServer.Configuration.EF.Migrations
{
    [DbContext(typeof(SimpleIdentityServerConfigurationContext))]
    partial class SimpleIdentityServerConfigurationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rc2-20901")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SimpleIdentityServer.Configuration.EF.Models.AuthenticationProvider", b =>
                {
                    b.Property<string>("Name");

                    b.Property<bool>("IsEnabled");

                    b.HasKey("Name");

                    b.ToTable("AuthenticationProviders");
                });

            modelBuilder.Entity("SimpleIdentityServer.Configuration.EF.Models.Option", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("AuthenticationProviderId");

                    b.Property<string>("Key");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("AuthenticationProviderId");

                    b.ToTable("Options");
                });

            modelBuilder.Entity("SimpleIdentityServer.Configuration.EF.Models.Option", b =>
                {
                    b.HasOne("SimpleIdentityServer.Configuration.EF.Models.AuthenticationProvider")
                        .WithMany()
                        .HasForeignKey("AuthenticationProviderId");
                });
        }
    }
}
