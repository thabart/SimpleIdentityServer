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

using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using System;

namespace SimpleIdentityServer.Host.Services
{
    public class DefaultAuthenticateResourceOwerService : IAuthenticateResourceOwnerService
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly ISecurityHelper _securityHelper;

        public DefaultAuthenticateResourceOwerService(
            IResourceOwnerRepository resourceOwnerRepository,
            ISecurityHelper securityHelper)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _securityHelper = securityHelper;
        }

        public ResourceOwner AuthenticateResourceOwner(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            return _resourceOwnerRepository.Get(login);
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

            return _resourceOwnerRepository.Get(login, _securityHelper.ComputeHash(password));
        }
    }
}
