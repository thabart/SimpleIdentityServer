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

using Microsoft.Data.Entity;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public static class ResourceOwnerRoleMapping
    {
        public static void AddResourceOwnerRoleMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResourceOwnerRole>()
                .ToTable("resourceOwnerRoles")
                .HasKey(r => new { r.ResourceOwnerId, r.RoleName });
            modelBuilder.Entity<ResourceOwnerRole>()
                .HasOne(r => r.ResourceOwner)
                .WithMany(r => r.ResourceOwnerRoles)
                .HasForeignKey(r => r.ResourceOwnerId);
            modelBuilder.Entity<ResourceOwnerRole>()
                .HasOne(r => r.Role)
                .WithMany(r => r.ResourceOwnerRoles)
                .HasForeignKey(r => r.RoleName);
        }
    }
}
