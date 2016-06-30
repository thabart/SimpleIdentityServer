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

using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using System;

namespace SimpleIdentityServer.Core.WebSite.Account.Actions
{
    public interface IAddResourceOwnerAction
    {
        void Execute(AddUserParameter addUserParameter);
    }

    internal class AddResourceOwnerAction : IAddResourceOwnerAction
    {
        #region Fields

        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        private readonly ISecurityHelper _securityHelper;

        #endregion

        #region Constructor

        public AddResourceOwnerAction(
            IResourceOwnerRepository resourceOwnerRepository,
            ISecurityHelper securityHelper)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _securityHelper = securityHelper;
        }

        #endregion

        #region Public methods

        public void Execute(AddUserParameter addUserParameter)
        {
            if (addUserParameter == null)
            {
                throw new ArgumentNullException(nameof(addUserParameter));
            }

            if (string.IsNullOrEmpty(addUserParameter.Name))
            {
                throw new ArgumentNullException(nameof(addUserParameter.Name));
            }

            if (string.IsNullOrWhiteSpace(addUserParameter.Password))
            {
                throw new ArgumentNullException(nameof(addUserParameter.Password));
            }

            var hashedPassword = _securityHelper.ComputeHash(addUserParameter.Password);
            if (_resourceOwnerRepository.GetResourceOwnerByCredentials(addUserParameter.Name, hashedPassword) != null)
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.UnhandledExceptionCode,
                    Errors.ErrorDescriptions.TheRoWithCredentialsAlreadyExists);
            }

            var newResourceOwner = new ResourceOwner
            {
                Id = Guid.NewGuid().ToString(),
                Name = addUserParameter.Name,
                Password = hashedPassword,
                IsLocalAccount = true
            };
            _resourceOwnerRepository.Insert(newResourceOwner);
        }

        #endregion
    }
}
