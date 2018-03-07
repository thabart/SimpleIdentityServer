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
using SimpleIdentityServer.Scim.Db.EF.Mappings;
using SimpleIdentityServer.Scim.Db.EF.Models;
using System;
using System.Linq;

namespace SimpleIdentityServer.Scim.Db.EF
{
    public class ScimDbContext : DbContext
    {
        public ScimDbContext(DbContextOptions<ScimDbContext> dbContextOptions) : base(dbContextOptions) { }
    
        public virtual DbSet<Representation> Representations { get; set; }
        public virtual DbSet<Schema> Schemas { get; set; }
        public virtual DbSet<SchemaAttribute> SchemaAttributes { get; set; }
        public virtual DbSet<RepresentationAttribute> RepresentationAttributes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddRepresentationAttributeMappings()
                .AddRepresentationMappings()
                .AddSchemaAttributeMappings()
                .AddSchemaMappings()
                .AddMetaDataMappings();
            base.OnModelCreating(builder);
        }
    }
}
