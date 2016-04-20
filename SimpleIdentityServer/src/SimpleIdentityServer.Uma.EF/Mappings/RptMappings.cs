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
using SimpleIdentityServer.Uma.EF.Models;

namespace SimpleIdentityServer.Uma.EF.Mappings
{
    internal static class RptMapping
    {
        #region Public static methods

        public static void AddRptMappings(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rpt>()
                .ToTable("Rpts")
                .HasKey(r => r.Value);
            modelBuilder.Entity<Rpt>()
                .HasOne(r => r.ResourceSet)
                .WithMany(r => r.Rpts)
                .HasForeignKey(r => r.ResourceSetId);
            modelBuilder.Entity<Rpt>()
                .HasOne(r => r.Ticket)
                .WithMany(r => r.Rpts)
                .HasForeignKey(r => r.TicketId);
        }

        #endregion
    }
}
