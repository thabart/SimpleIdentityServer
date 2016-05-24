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
    public static class ConsentMapping
    {
        public static void AddConsentMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Consent>()
                .ToTable("consents")
                .HasKey(c => c.Id);
            modelBuilder.Entity<Consent>()
                .HasOne(c => c.Client)
                .WithMany(c => c.Consents)
                .HasForeignKey(c => c.ClientId);
            modelBuilder.Entity<Consent>()
                .HasOne(c => c.ResourceOwner)
                .WithMany(c => c.Consents)
                .HasForeignKey(c => c.ResourceOwnerId);
            /*
            ToTable("consents");
            HasKey(c => c.Id);
            HasRequired(c => c.Client)
                .WithMany(c => c.Consents);
            HasRequired(c => c.ResourceOwner)
                .WithMany(r => r.Consents);
            HasMany(c => c.GrantedScopes)
                .WithMany(s => s.Consents)
                .Map(c =>
                {
                    c.MapLeftKey("ConsentId");
                    c.MapRightKey("ScopeName");
                    c.ToTable("consentScopes");
                });
            HasMany(c => c.Claims)
                .WithMany(c => c.Consents)
                .Map(c =>
                {
                    c.MapLeftKey("ConsentId");
                    c.MapRightKey("ClaimCode");
                    c.ToTable("consentClaims");
                });
           */
        }
    }
}
