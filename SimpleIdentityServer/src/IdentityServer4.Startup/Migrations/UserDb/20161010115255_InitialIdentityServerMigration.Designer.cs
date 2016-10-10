using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SimpleIdentityServer.IdentityServer.EF.DbContexts;

namespace IdentityServer4.Startup.Migrations.UserDb
{
    [DbContext(typeof(UserDbContext))]
    [Migration("20161010115255_InitialIdentityServerMigration")]
    partial class InitialIdentityServerMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("SimpleIdentityServer.IdentityServer.EF.Models.Claim", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Key");

                    b.Property<string>("UserSubject")
                        .IsRequired();

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("UserSubject");

                    b.ToTable("Claims");
                });

            modelBuilder.Entity("SimpleIdentityServer.IdentityServer.EF.Models.User", b =>
                {
                    b.Property<string>("Subject");

                    b.Property<bool>("Enabled");

                    b.Property<bool>("IsLocalAccount");

                    b.Property<string>("Password");

                    b.Property<string>("Provider");

                    b.Property<string>("ProviderId");

                    b.Property<string>("Username");

                    b.HasKey("Subject");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SimpleIdentityServer.IdentityServer.EF.Models.Claim", b =>
                {
                    b.HasOne("SimpleIdentityServer.IdentityServer.EF.Models.User", "User")
                        .WithMany("Claims")
                        .HasForeignKey("UserSubject")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
