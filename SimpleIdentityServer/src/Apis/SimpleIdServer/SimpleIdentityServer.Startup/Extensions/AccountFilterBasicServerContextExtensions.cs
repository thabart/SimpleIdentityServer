using SimpleIdentityServer.AccountFilter.Basic.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Startup.Extensions
{
    public static class AccountFilterBasicServerContextExtensions
    {
        public static void EnsureSeedData(this AccountFilterBasicServerContext context)
        {
            AddFilters(context);
            context.SaveChanges();
        }

        private static void AddFilters(AccountFilterBasicServerContext context)
        {
            if (!context.Filters.Any())
            {
                context.Filters.Add(new AccountFilter.Basic.EF.Models.Filter
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateDateTime = DateTime.Now,
                    UpdateDateTime = DateTime.Now,
                    Name = "is_admin",
                    Rules = new List<AccountFilter.Basic.EF.Models.FilterRule>
                    {
                        new AccountFilter.Basic.EF.Models.FilterRule
                        {
                            ClaimKey = "phone_number",
                            ClaimValue = "+32485350536",
                            Operation = 0
                        }
                    }
                });
            }
        }
    }
}
