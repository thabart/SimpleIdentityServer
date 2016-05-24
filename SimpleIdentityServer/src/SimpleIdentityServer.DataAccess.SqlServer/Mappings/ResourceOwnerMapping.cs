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
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public static class ResourceOwnerMapping
    {
        public static void AddResourceOwnerMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResourceOwner>()
                .ToTable("resourceOwners")
                .HasKey(j => j.Id);
            modelBuilder.Entity<ResourceOwner>()
                .HasOne(r => r.Address)
                .WithOne(a => a.ResourceOwner)
                .HasForeignKey<Address>(a => a.ResourceOwnerForeignKey);
            /*
            ToTable("resourceOwners");
            HasKey(r => r.Id);
            Property(r => r.Name);
            Property(r => r.GivenName);
            Property(r => r.FamilyName);
            Property(r => r.MiddleName);
            Property(r => r.NickName);
            Property(r => r.PreferredUserName);
            Property(r => r.Profile);
            Property(r => r.Picture);
            Property(r => r.WebSite);
            Property(r => r.Email);
            Property(r => r.EmailVerified);
            Property(r => r.Gender);
            Property(r => r.BirthDate);
            Property(r => r.ZoneInfo);
            Property(r => r.Locale);
            Property(r => r.PhoneNumber);
            Property(r => r.PhoneNumberVerified);
            Property(r => r.UpdatedAt);
            Property(r => r.Password);
            HasOptional(r => r.Address)
                .WithRequired(a => a.ResourceOwner);
            */
        }
    }
}
