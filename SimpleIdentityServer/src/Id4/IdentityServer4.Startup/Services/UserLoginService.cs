#region copyright
// Copyright 2015 Habart Thierry
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

using IdentityModel;
using IdentityServer4.Startup.Extensions;
using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.IdentityServer.EF.DbContexts;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityServer4.Startup.Services
{
    public class UserLoginService
    {
        private readonly UserDbContext _context;

        public UserLoginService(UserDbContext context)
        {
            if(context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;
        }

        public bool ValidateCredentials(string userName, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == userName && u.Password == ComputeHash(password));
            return user != null;
        }

        public DbUser FindUserName(string userName)
        {
            var user = _context.Users.Include(u => u.Claims).FirstOrDefault(u => u.Username == userName);
            if (user == null)
            {
                return null;
            }

            return new DbUser
            {
                Username = user.Username,
                Enabled = user.Enabled,
                IsLocalAccount = user.IsLocalAccount,
                Password = user.Password,
                Provider = user.Provider,
                ProviderId = user.ProviderId,
                Subject = user.Subject,
                Claims = user.Claims == null ? new List<DbClaim>() : user.Claims.Select(c => new DbClaim(c.Key, c.Value)).ToList()
            };
        }

        public DbUser FindByExternalProvider(string provider, string userId)
        {
            var user = _context.Users.Include(u => u.Claims).FirstOrDefault(u => u.ProviderId == userId && u.Provider == provider);
            if (user == null)
            {
                return null;
            }

            return new DbUser
            {
                Username = user.Username,
                Enabled = user.Enabled,
                IsLocalAccount = user.IsLocalAccount,
                Password = user.Password,
                Provider = user.Provider,
                ProviderId = user.ProviderId,
                Subject = user.Subject,
                Claims = user.Claims == null ? new List<DbClaim>() : user.Claims.Select(c => new DbClaim(c.Key, c.Value)).ToList()
            };
        }

        public DbUser AutoProvisionUser(string provider, string userId, List<Claim> claims)
        {
            var filtered = new List<DbClaim>();
            foreach (var claim in claims)
            {
                if (claim.Type == ClaimTypes.Name)
                {
                    filtered.Add(new DbClaim(JwtClaimTypes.Name, claim.Value));
                }
                else if (JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.ContainsKey(claim.Type))
                {
                    filtered.Add(new DbClaim(JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap[claim.Type], claim.Value));
                }
                else
                {
                    filtered.Add(new DbClaim(claim.Type, claim.Value));
                }
            }

            if (!filtered.Any(x => x.Key == JwtClaimTypes.Name))
            {
                var first = filtered.FirstOrDefault(x => x.Key == JwtClaimTypes.GivenName)?.Value;
                var last = filtered.FirstOrDefault(x => x.Key == JwtClaimTypes.FamilyName)?.Value;
                if (first != null && last != null)
                {
                    filtered.Add(new DbClaim(JwtClaimTypes.Name, first + " " + last));
                }
                else if (first != null)
                {
                    filtered.Add(new DbClaim(JwtClaimTypes.Name, first));
                }
                else if (last != null)
                {
                    filtered.Add(new DbClaim(JwtClaimTypes.Name, last));
                }
            }

            var sub = CryptoRandom.CreateUniqueId();
            var name = filtered.FirstOrDefault(c => c.Key == JwtClaimTypes.Name)?.Value ?? sub;
            var user = new DbUser()
            {
                Enabled = true,
                Subject = sub,
                Username = name,
                Provider = provider,
                ProviderId = userId,
                Claims = filtered
            };

            _context.Users.Add(user.ToEntity());
            _context.SaveChanges();
            return user;
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
