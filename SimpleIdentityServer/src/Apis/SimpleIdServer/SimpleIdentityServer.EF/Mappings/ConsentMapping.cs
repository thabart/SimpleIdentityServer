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
using SimpleIdentityServer.EF.Models;

namespace SimpleIdentityServer.EF.Mappings
{
    public static class ConsentMapping
    {
        #region Public static methods

        public static void AddConsentMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Consent>()
                .ToTable("consents")
                .HasKey(c => c.Id);
            modelBuilder.Entity<Consent>()
                .HasMany(r => r.ConsentScopes)
                .WithOne(a => a.Consent)
                .HasForeignKey(fk => fk.ConsentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Consent>()
                .HasMany(r => r.ConsentClaims)
                .WithOne(a => a.Consent)
                .HasForeignKey(fk => fk.ConsentId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
