using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using System.Linq;

namespace IdentityServer4.Startup.Services
{
    public class UserProfileService : IProfileService
    {
        #region Constructor

        public UserProfileService()
        {

        }

        #endregion

        #region Public methods
        
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var claims = context.Subject.Claims;
            if (!context.AllClaimsRequested || !context.RequestedClaimTypes.Any())
            {
                claims = claims.Where(x => context.RequestedClaimTypes.Contains(x.Type));
            }

            context.IssuedClaims = claims;
            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.FromResult(0);
        }

        #endregion
    }
}
