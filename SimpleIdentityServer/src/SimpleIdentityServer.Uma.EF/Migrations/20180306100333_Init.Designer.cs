using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SimpleIdentityServer.Uma.EF;

namespace SimpleIdentityServer.Uma.EF.Migrations
{
    [DbContext(typeof(SimpleIdServerUmaContext))]
    [Migration("20180306100333_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.3")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.Policy", b =>
                {
                    b.Property<string>("Id");

                    b.HasKey("Id");

                    b.ToTable("Policies");
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.PolicyResource", b =>
                {
                    b.Property<string>("PolicyId");

                    b.Property<string>("ResourceSetId");

                    b.HasKey("PolicyId", "ResourceSetId");

                    b.HasIndex("PolicyId");

                    b.HasIndex("ResourceSetId");

                    b.ToTable("PolicyResource");
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.PolicyRule", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Claims");

                    b.Property<string>("ClientIdsAllowed");

                    b.Property<bool>("IsResourceOwnerConsentNeeded");

                    b.Property<string>("PolicyId");

                    b.Property<string>("Scopes");

                    b.Property<string>("Script");

                    b.HasKey("Id");

                    b.HasIndex("PolicyId");

                    b.ToTable("PolicyRules");
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.ResourceSet", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("IconUri");

                    b.Property<string>("Name");

                    b.Property<string>("Scopes");

                    b.Property<string>("Type");

                    b.Property<string>("Uri");

                    b.HasKey("Id");

                    b.ToTable("ResourceSets");
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.PolicyResource", b =>
                {
                    b.HasOne("SimpleIdentityServer.Uma.EF.Models.Policy", "Policy")
                        .WithMany("PolicyResources")
                        .HasForeignKey("PolicyId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SimpleIdentityServer.Uma.EF.Models.ResourceSet", "ResourceSet")
                        .WithMany("PolicyResources")
                        .HasForeignKey("ResourceSetId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.PolicyRule", b =>
                {
                    b.HasOne("SimpleIdentityServer.Uma.EF.Models.Policy", "Policy")
                        .WithMany("Rules")
                        .HasForeignKey("PolicyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
