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
using SimpleIdentityServer.IdentityServer.EF.Extensions;
using SimpleIdentityServer.IdentityServer.EF.Models;

namespace SimpleIdentityServer.IdentityServer.EF.DbContexts
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        #region Public properties
        
        public DbSet<User> Users { get; set; }

        #endregion

        #region Protected methods

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ConfigureUserContext();
            base.OnModelCreating(builder);
        }

        #endregion
    }
}
