using System;
using System.Threading.Tasks;
using IdentityServer4.Validation;
using SimpleIdentityServer.IdentityServer.EF.DbContexts;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer4.Startup.Validation
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserDbContext _context;

        public ResourceOwnerPasswordValidator(UserDbContext context)
        {
            _context = context;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = _context.Users.Include(u => u.Claims)
                .FirstOrDefault(u => u.Username == context.UserName && u.Password == context.Password);
            if (user != null)
            {
                var claims = user.Claims == null || !user.Claims.Any() ? new List<Claim>() : user.Claims.Select(c => new Claim(c.Key, c.Value)).ToList();
                context.Result = new GrantValidationResult(user.Subject, "password", claims);
            }

            return Task.FromResult(0);
        }
    }
}
