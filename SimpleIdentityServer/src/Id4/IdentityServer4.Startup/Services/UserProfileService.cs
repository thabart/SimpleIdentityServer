using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.IdentityServer.EF.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Startup.Services
{
    public class UserProfileService : IProfileService
    {
        private readonly UserDbContext _context;

        #region Constructor

        public UserProfileService(UserDbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;
        }

        #endregion

        #region Public methods
        
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = _context.Users.Include(u => u.Claims).FirstOrDefault(u => u.Subject == context.Subject.GetSubjectId());
            if (user != null)
            {
                var claims = user.Claims == null || !user.Claims.Any() ? new List<Claim>() : user.Claims.Select(c => new Claim(c.Key, c.Value)).ToList();
                var userClaims = new List<Claim>();
                userClaims.Add(new Claim(JwtClaimTypes.Subject, user.Subject));
                if (!context.AllClaimsRequested)
                {
                    userClaims.AddRange(claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList());
                }
                else
                {
                    userClaims.AddRange(claims);
                }

                context.IssuedClaims = userClaims;
            }

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
