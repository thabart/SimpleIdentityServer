using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.AccountFilter.Basic.EF.Mappings;
using SimpleIdentityServer.AccountFilter.Basic.EF.Models;

namespace SimpleIdentityServer.AccountFilter.Basic.EF
{
    public class AccountFilterBasicServerContext : DbContext
    {
        #region Constructor

        public AccountFilterBasicServerContext(DbContextOptions<AccountFilterBasicServerContext> dbContextOptions):base(dbContextOptions)
        {
        }

        #endregion

        public DbSet<Filter> Filters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddFilterMapping()
                .AddFilterRuleMapping();
            base.OnModelCreating(modelBuilder);
        }
    }
}