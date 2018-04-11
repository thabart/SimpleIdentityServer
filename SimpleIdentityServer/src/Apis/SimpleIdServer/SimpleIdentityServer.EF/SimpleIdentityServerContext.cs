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

using SimpleIdentityServer.EF.Mappings;
using SimpleIdentityServer.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace SimpleIdentityServer.EF
{
    public class SimpleIdentityServerContext : DbContext
    {
        #region Constructor

        public SimpleIdentityServerContext(DbContextOptions<SimpleIdentityServerContext> dbContextOptions):base(dbContextOptions)
        {
        }

        #endregion

        public virtual DbSet<Translation> Translations { get; set; }
        public virtual DbSet<Scope> Scopes { get; set; }
        public virtual DbSet<Claim> Claims { get; set; }
        public virtual DbSet<ResourceOwner> ResourceOwners { get; set; }
        public virtual DbSet<JsonWebKey> JsonWebKeys { get; set; } 
        public virtual DbSet<Models.Client> Clients { get; set; } 
        public virtual DbSet<Consent> Consents { get; set; }
        public virtual DbSet<ClientScope> ClientScopes { get; set; }        
        public virtual DbSet<ConsentClaim> ConsentClaims { get; set; }
        public virtual DbSet<ConsentScope> ConsentScopes { get; set; }
        public virtual DbSet<ScopeClaim> ScopeClaims { get; set; }
        public virtual DbSet<ResourceOwnerClaim> ResourceOwnerClaims { get; set; }
        public virtual DbSet<ConfirmationCode> ConfirmationCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddClaimMapping();
            modelBuilder.AddClientMapping();
            modelBuilder.AddConsentClaimMapping();
            modelBuilder.AddConsentMapping();
            modelBuilder.AddConsentScopeMapping();
            modelBuilder.AddJsonWebKeyMapping();
            modelBuilder.AddResourceOwnerMapping();
            modelBuilder.AddScopeClaimMapping();
            modelBuilder.AddScopeMapping();
            modelBuilder.AddTranslationMapping();
            modelBuilder.AddClientScopeMapping();
            modelBuilder.AddResourceOwnerClaimMapping();
            modelBuilder.AddConfirmationCodeMapping();
            modelBuilder.AddClientSecretMapping();
            base.OnModelCreating(modelBuilder);
        }
    }
}