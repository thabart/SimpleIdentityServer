using System;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

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
            throw new NotImplementedException();
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
