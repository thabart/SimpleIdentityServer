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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Db.EF;
using SimpleIdentityServer.Scim.Db.EF.Extensions;
using SimpleIdentityServer.Scim.Db.EF.Helpers;
using SimpleIdentityServer.Scim.Db.EF.Stores;
using System;

namespace SimpleIdentityServer.Scim.Core.Tests.Fixture
{
    public class StoresFixture : IDisposable
    {
        public StoresFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
            var builder = new DbContextOptionsBuilder<ScimDbContext>();
            builder.UseInMemoryDatabase()
                .UseInternalServiceProvider(serviceProvider)
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            var ctx = new ScimDbContext(builder.Options);
            ctx.EnsureSeedData();
            SchemaStore = new SchemaStore(ctx, new Transformers(ctx));
        }

        public ISchemaStore SchemaStore { get; private set; }

        public void Dispose()
        {
            SchemaStore.Dispose();
        }
    }
}
