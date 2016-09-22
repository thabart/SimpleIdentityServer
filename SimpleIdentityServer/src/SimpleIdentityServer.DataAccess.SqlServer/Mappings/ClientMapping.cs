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
    public static class ClientMapping
    {
        public static void AddClientMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Client>()
                .ToTable("clients")
                .HasKey(c => c.ClientId);
            /*
            ToTable("clients");
            HasKey(c => c.ClientId);
            Property(c => c.ClientSecret);
            Property(c => c.ClientName);
            Property(c => c.LogoUri);
            Property(c => c.ClientUri);
            Property(c => c.PolicyUri);
            Property(c => c.TosUri);
            Property(c => c.IdTokenSignedResponseAlg);
            Property(c => c.IdTokenEncryptedResponseAlg);
            Property(c => c.IdTokenEncryptedResponseEnc);
            Property(c => c.TokenEndPointAuthMethod);
            Property(c => c.ResponseTypes);
            Property(c => c.GrantTypes);
            Property(c => c.RedirectionUrls);
            Property(c => c.ApplicationType);
            Property(c => c.JwksUri);
            Property(c => c.Contacts);
            Property(c => c.SectorIdentifierUri);
            Property(c => c.SubjectType);
            Property(c => c.UserInfoSignedResponseAlg);
            Property(c => c.UserInfoEncryptedResponseAlg);
            Property(c => c.UserInfoEncryptedResponseEnc);
            Property(c => c.RequestObjectSigningAlg);
            Property(c => c.RequestObjectEncryptionAlg);
            Property(c => c.RequestObjectEncryptionEnc);
            Property(c => c.TokenEndPointAuthSigningAlg);
            Property(c => c.DefaultMaxAge);
            Property(c => c.RequireAuthTime);
            Property(c => c.DefaultAcrValues);
            Property(c => c.InitiateLoginUri);
            Property(c => c.RequestUris);
            // Set AllowedScopes & JsonWebKeys
            */            
        }
    }
}
