using IdentityServer4.Startup.Services;
using SimpleIdentityServer.IdentityServer.EF.Models;
using System.Linq;

namespace IdentityServer4.Startup.Extensions
{
    internal static class MappingExtensions
    {
        public static User ToEntity(this DbUser user)
        {
            var result = new User
            {
                Enabled = user.Enabled,
                IsLocalAccount = user.IsLocalAccount,
                Password = user.Password,
                Provider = user.Provider,
                ProviderId = user.ProviderId,
                Subject = user.Subject,
                Username = user.Username
            };

            if (user.Claims != null && user.Claims.Any())
            {
                result.Claims = user.Claims.Select(c => c.ToEntity()).ToList();
            }

            return result;
        }

        public static Claim ToEntity(this DbClaim claim)
        {
            return new Claim
            {
                Id = claim.Id,
                Key = claim.Key,
                Value = claim.Value
            };
        }
    }
}
