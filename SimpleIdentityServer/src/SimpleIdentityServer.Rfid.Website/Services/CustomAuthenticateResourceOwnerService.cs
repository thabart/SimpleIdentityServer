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

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SimpleIdentityServer.Rfid.Website.Services
{
    public class CustomAuthenticateResourceOwnerService : IAuthenticateResourceOwnerService
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public CustomAuthenticateResourceOwnerService(
            IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public ResourceOwner AuthenticateResourceOwner(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            var resourceOwners = _resourceOwnerRepository.GetResourceOwners(new List<Claim>
            {
                new Claim("card_number", login)
            });
            return !resourceOwners.Any() ? null : resourceOwners.First();
        }

        public ResourceOwner AuthenticateResourceOwner(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var resourceOwners = _resourceOwnerRepository.GetResourceOwners(new List<Claim>
            {
                new Claim("card_number", login)
            }).Where(r => r.Password == GetHashedPassword(password));
            return !resourceOwners.Any() ? null : resourceOwners.First();
        }

        public string GetHashedPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            return ComputeHash(password);
        }

        private string ComputeHash(string entry)
        {
            using (var sha256 = SHA256.Create())
            {
                var entryBytes = Encoding.UTF8.GetBytes(entry);
                var hash = sha256.ComputeHash(entryBytes);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}
