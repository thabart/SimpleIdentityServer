#region copyright
// Copyright 2017 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Threading.Tasks;
using IdentityServer4.Validation;
using SimpleIdentityServer.IdentityServer.EF.DbContexts;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System;

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
                .FirstOrDefault(u => u.Username == context.UserName && u.Password == ComputeHash(context.Password));
            if (user != null)
            {
                var claims = user.Claims == null || !user.Claims.Any() ? new List<Claim>() : user.Claims.Select(c => new Claim(c.Key, c.Value)).ToList();
                context.Result = new GrantValidationResult(user.Subject, "password", claims);
            }

            return Task.FromResult(0);
        }

        private static string ComputeHash(string entry)
        {
            using (var sha256 = SHA256.Create())
            {
                var entryBytes = Encoding.UTF8.GetBytes(entry);
                var hash = sha256.ComputeHash(entryBytes);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}
