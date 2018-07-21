﻿#region copyright
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
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.ResourceOwners.Actions
{
    public interface IGetResourceOwnerAction
    {
        Task<ResourceOwner> Execute(string subject);
    }

    internal class GetResourceOwnerAction : IGetResourceOwnerAction
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public GetResourceOwnerAction(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<ResourceOwner> Execute(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var resourceOwner = await _resourceOwnerRepository.GetAsync(subject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, subject));
            }

            return resourceOwner;
        }
    }
}
