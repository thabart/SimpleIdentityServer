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

using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;

namespace SimpleIdentityServer.Manager.Core.Api.ResourceOwners.Actions
{
    public interface IUpdateResourceOwnerAction
    {
        bool Execute(UpdateResourceOwnerParameter updateResourceOwnerParameter);
    }

    internal class UpdateResourceOwnerAction : IUpdateResourceOwnerAction
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        #region Constructor

        public UpdateResourceOwnerAction(
            IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        #endregion

        #region Public methods

        public bool Execute(UpdateResourceOwnerParameter updateResourceOwnerParameter)
        {
            if (updateResourceOwnerParameter == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerParameter));
            }

            if (string.IsNullOrWhiteSpace(updateResourceOwnerParameter.Subject))
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerParameter.Subject));
            }

            var resourceOwner = _resourceOwnerRepository.GetBySubject(updateResourceOwnerParameter.Subject);
            if (resourceOwner == null)
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidParameterCode,
                    string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, updateResourceOwnerParameter.Subject));
            }

            if (!resourceOwner.IsLocalAccount)
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.TheResourceOwnerMustBeConfirmed);
            }

            resourceOwner.Roles = updateResourceOwnerParameter.Roles;
            return _resourceOwnerRepository.Update(resourceOwner);
        }

        #endregion
    }
}
