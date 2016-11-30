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
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using System;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdentityServer.Manager.Core.Api.ResourceOwners.Actions
{
    public interface IUpdateResourceOwnerAction
    {
        bool Execute(ResourceOwner resourceOwner);
    }

    internal class UpdateResourceOwnerAction : IUpdateResourceOwnerAction
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        
        public UpdateResourceOwnerAction(
            IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public bool Execute(ResourceOwner parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(parameter.Id))
            {
                throw new ArgumentNullException(nameof(parameter.Id));
            }

            if (_resourceOwnerRepository.GetByUniqueClaim(parameter.Id) == null)
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidParameterCode,
                    string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, parameter.Id));
            }

            if (parameter.Claims != null)
            {
                Claim updatedClaim;
                if (((updatedClaim = parameter.Claims.FirstOrDefault(c => c.Type == SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt)) != null))
                {
                    parameter.Claims.Remove(updatedClaim);
                }

                parameter.Claims.Add(new Claim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()));
            }

            return _resourceOwnerRepository.Update(parameter);
        }
    }
}
