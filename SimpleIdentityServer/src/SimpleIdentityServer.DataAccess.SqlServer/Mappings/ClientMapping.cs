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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public static class ClientMapping
    {
        public static void AddClientMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Client>()
                .ToTable("clients")
                .HasKey(c => c.ClientId);
            modelBuilder.Entity<Models.Client>()
                .HasMany(c => c.ClientScopes)
                .WithOne(s => s.Client)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Models.Client>()
                .HasMany(c => c.JsonWebKeys)
                .WithOne(s => s.Client)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Models.Client>()
                .HasMany(c => c.Consents)
                .WithOne(s => s.Client)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Models.Client>()
                .HasMany(c => c.ClientSecrets)
                .WithOne(s => s.Client)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
