using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using SimpleIdentityServer.Uma.EF;

namespace SimpleIdentityServer.Uma.EF.Migrations
{
    [DbContext(typeof(SimpleIdServerUmaContext))]
    partial class SimpleIdServerUmaContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.Policy", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ClientIdsAllowed");

                    b.Property<bool>("IsCustom");

                    b.Property<bool>("IsResourceOwnerConsentNeeded");

                    b.Property<string>("Scopes");

                    b.Property<string>("Script");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Policies");
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.ResourceSet", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("IconUri");

                    b.Property<string>("Name");

                    b.Property<string>("PolicyId");

                    b.Property<string>("Scopes");

                    b.Property<string>("Type");

                    b.Property<string>("Uri");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "ResourceSets");
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.Rpt", b =>
                {
                    b.Property<string>("Value");

                    b.Property<DateTime>("CreateDateTime");

                    b.Property<DateTime>("ExpirationDateTime");

                    b.Property<string>("ResourceSetId");

                    b.Property<string>("TicketId");

                    b.HasKey("Value");

                    b.HasAnnotation("Relational:TableName", "Rpts");
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.Scope", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("IconUri");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Scopes");
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.Ticket", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ClientId");

                    b.Property<DateTime>("CreateDateTime");

                    b.Property<DateTime>("ExpirationDateTime");

                    b.Property<string>("ResourceSetId");

                    b.Property<string>("Scopes");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Tickets");
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.ResourceSet", b =>
                {
                    b.HasOne("SimpleIdentityServer.Uma.EF.Models.Policy")
                        .WithMany()
                        .HasForeignKey("PolicyId");
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.Rpt", b =>
                {
                    b.HasOne("SimpleIdentityServer.Uma.EF.Models.ResourceSet")
                        .WithMany()
                        .HasForeignKey("ResourceSetId");

                    b.HasOne("SimpleIdentityServer.Uma.EF.Models.Ticket")
                        .WithMany()
                        .HasForeignKey("TicketId");
                });

            modelBuilder.Entity("SimpleIdentityServer.Uma.EF.Models.Ticket", b =>
                {
                    b.HasOne("SimpleIdentityServer.Uma.EF.Models.ResourceSet")
                        .WithMany()
                        .HasForeignKey("ResourceSetId");
                });
        }
    }
}
