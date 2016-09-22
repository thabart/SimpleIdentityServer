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
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    public interface IUpdateUserOperation
    {
        void Execute(UpdateUserParameter updateUserParameter);
    }

    internal class UpdateUserOperation : IUpdateUserOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        private readonly ISecurityHelper _securityHelper;

        #region Constructor

        public UpdateUserOperation(
            IResourceOwnerRepository resourceOwnerRepository,
            ISecurityHelper securityHelper)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _securityHelper = securityHelper;
        }

        #endregion

        #region Public methods

        public void Execute(UpdateUserParameter updateUserParameter)
        {
            if (updateUserParameter == null)
            {
                throw new ArgumentNullException(nameof(updateUserParameter));
            }

            if (string.IsNullOrWhiteSpace(updateUserParameter.Id))
            {
                throw new ArgumentNullException(nameof(updateUserParameter.Id));
            }

            if (string.IsNullOrWhiteSpace(updateUserParameter.Name))
            {
                throw new ArgumentNullException(nameof(updateUserParameter.Name));
            }

            if (string.IsNullOrWhiteSpace(updateUserParameter.Password))
            {
                throw new ArgumentNullException(nameof(updateUserParameter.Password));
            }

            if (!string.IsNullOrWhiteSpace(updateUserParameter.Email) && !new EmailAddressAttribute().IsValid(updateUserParameter.Email))
            {
                throw new ArgumentException($"not a valid email address {updateUserParameter.Email}");
            }

            var newPassword = _securityHelper.ComputeHash(updateUserParameter.Password);
            var user = _resourceOwnerRepository.GetResourceOwnerByCredentials(updateUserParameter.Name,
                newPassword);
            if (user != null && user.Id != updateUserParameter.Id)
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.InternalError,
                    Errors.ErrorDescriptions.TheRoWithCredentialsAlreadyExists);
            }

            var resourceOwner = _resourceOwnerRepository.GetBySubject(updateUserParameter.Id);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.InternalError,
                    Errors.ErrorDescriptions.TheRoDoesntExist);
            }

            resourceOwner.Name = updateUserParameter.Name;
            resourceOwner.Email = updateUserParameter.Email;
            resourceOwner.TwoFactorAuthentication = updateUserParameter.TwoFactorAuthentication;
            resourceOwner.Password = newPassword;
            _resourceOwnerRepository.Update(resourceOwner);
        }

        #endregion
    }
}
