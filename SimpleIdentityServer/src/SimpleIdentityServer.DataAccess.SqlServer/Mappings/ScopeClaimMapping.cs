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
    public static class ScopeClaimMapping
    {
        public static void AddScopeClaimMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScopeClaim>()
                .ToTable("scopeClaims")
                .HasKey(s => new { s.ClaimCode, s.ScopeName });
            modelBuilder.Entity<ScopeClaim>()
                .HasOne(s => s.Scope)
                .WithMany(s => s.ScopeClaims)
                .HasForeignKey(s => s.ScopeName);
            modelBuilder.Entity<ScopeClaim>()
                .HasOne(s => s.Claim)
                .WithMany(s => s.ScopeClaims)
                .HasForeignKey(s => s.ClaimCode);
        }
    }
}
