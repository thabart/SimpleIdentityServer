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
    public static class JsonWebKeyMapping
    {
        public static void AddJsonWebKeyMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JsonWebKey>()
                .ToTable("jsonWebKeys")
                .HasKey(j => j.Kid);
            modelBuilder.Entity<JsonWebKey>()
                .HasOne(c => c.Client)
                .WithMany(c => c.JsonWebKeys)
                .HasForeignKey(c => c.ClientId);
            /*
            ToTable("jsonWebKeys");
            HasKey(j => j.Kid);
            Property(j => j.Kty);
            Property(j => j.Use);
            Property(j => j.KeyOps);
            Property(j => j.Alg);
            Property(j => j.X5u);
            Property(j => j.X5t);
            Property(j => j.X5tS256);
            Property(j => j.SerializedKey);
            */
        }
    }
}
