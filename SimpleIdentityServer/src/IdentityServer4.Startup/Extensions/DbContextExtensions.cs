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

using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Startup.Services;
using SimpleIdentityServer.IdentityServer.EF.DbContexts;
using System;
using System.Linq;

namespace IdentityServer4.Startup.Extensions
{
    internal static class DbContextExtensions
    {
        public static void EnsureSeedData(this ConfigurationDbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            CreateClients(context);
        }

        public static void EnsureSeedData(this UserDbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            CreateUsers(context);
        }

        private static void CreateScopes(ConfigurationDbContext context)
        {
            if (!context.Scopes.Any())
            {
                foreach (var s in Config.GetScopes())
                {
                    context.Scopes.Add(s.ToEntity());
                }

                context.SaveChanges();
            }
        }

        private static void CreateClients(ConfigurationDbContext context)
        {
            if (!context.Clients.Any())
            {
                foreach (var client in Config.GetClients())
                {
                    context.Clients.Add(client.ToEntity());
                }

                context.SaveChanges();
            }
        }

        private static void CreateUsers(UserDbContext context)
        {
            if (!context.Users.Any())
            {
                foreach (DbUser u in Config.GetUsers())
                {
                    context.Users.Add(u.ToEntity());
                }

                context.SaveChanges();
            }
        }
    }
}
