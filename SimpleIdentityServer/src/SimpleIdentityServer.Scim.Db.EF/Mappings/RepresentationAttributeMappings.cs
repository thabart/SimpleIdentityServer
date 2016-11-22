#region copyright
// Copyright 2016 Habart Thierry
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
using SimpleIdentityServer.Scim.Db.EF.Models;
using System;

namespace SimpleIdentityServer.Scim.Db.EF.Mappings
{
    internal static class RepresentationAttributeMappings
    {
        public static ModelBuilder AddRepresentationAttributeMappings(this ModelBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            
            builder.Entity<RepresentationAttribute>()
                .ToTable("representationAttributes")
                .HasKey(a => a.Id);
            builder.Entity<RepresentationAttribute>()
                .HasMany(a => a.Children)
                .WithOne(a => a.Parent)
                .HasForeignKey(a => a.RepresentationAttributeIdParent);
            return builder;
        }
    }
}
