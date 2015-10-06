using System.Data.Entity.ModelConfiguration;

using SimpleIdentityServer.Core.DataAccess.Models;

namespace SimpleIdentityServer.DataAccess.Mappings
{
    public class ClientMapping : EntityTypeConfiguration<Client>
    {
        public ClientMapping()
        {
            ToTable("Clients");
        }
    }
}
