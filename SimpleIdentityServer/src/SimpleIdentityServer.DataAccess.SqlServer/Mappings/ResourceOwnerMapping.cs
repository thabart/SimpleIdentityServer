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
    public static class ResourceOwnerMapping
    {
        #region Public static methods

        public static void AddResourceOwnerMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResourceOwner>()
                .ToTable("resourceOwners")
                .HasKey(j => j.Id);
            modelBuilder.Entity<ResourceOwner>()
                .HasOne(r => r.Address)
                .WithOne(a => a.ResourceOwner)
                .HasForeignKey<Address>(a => a.ResourceOwnerForeignKey)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ResourceOwner>()
                .HasMany(r => r.ResourceOwnerRoles)
                .WithOne(o => o.ResourceOwner)
                .HasForeignKey(a => a.ResourceOwnerId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ResourceOwner>()
                .HasMany(r => r.Consents)
                .WithOne(c => c.ResourceOwner)
                .HasForeignKey(a => a.ResourceOwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
