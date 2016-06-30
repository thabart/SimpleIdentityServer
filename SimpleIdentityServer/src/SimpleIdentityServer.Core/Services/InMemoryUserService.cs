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
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.Services
{
    public class InMemoryUserService : IResourceOwnerService
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        private readonly ISecurityHelper _securityHelper;

        public InMemoryUserService(
            IResourceOwnerRepository resourceOwnerRepository,
            ISecurityHelper securityHelper)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _securityHelper = securityHelper;
        }

        public string Authenticate(string userName, string password)
        {
            var hashedPassword = _securityHelper.ComputeHash(password);
            var user = _resourceOwnerRepository.GetResourceOwnerByCredentials(userName, hashedPassword);
            if (user == null || !user.IsLocalAccount)
            {
                return null;
            }

            return user.Id;
        }
    }
}
