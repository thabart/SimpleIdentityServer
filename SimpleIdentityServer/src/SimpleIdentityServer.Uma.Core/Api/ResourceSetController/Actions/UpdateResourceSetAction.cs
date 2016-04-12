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

using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Core.Validators;
using System;
using System.Net;

namespace SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions
{
    internal interface IUpdateResourceSetAction
    {
        /// <summary>
        /// throw an <see cref="ArgumentNullException"/> exception when the parameter is null.
        /// throw an <see cref="BaseUmaException"/> exception when an exception occured while trying to update the resource set.
        /// if the resource set doesn't exist then false is returned
        /// </summary>
        /// <param name="udpateResourceSetParameter"></param>
        /// <returns></returns>
        bool Execute(UpdateResourceSetParameter udpateResourceSetParameter);
    }

    internal class UpdateResourceSetAction : IUpdateResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        private readonly IResourceSetParameterValidator _resourceSetParameterValidator;

        #region Constructor

        public UpdateResourceSetAction(
            IResourceSetRepository resourceSetRepository,
            IResourceSetParameterValidator resourceSetParameterValidator)
        {
            _resourceSetRepository = resourceSetRepository;
            _resourceSetParameterValidator = resourceSetParameterValidator;
        }

        #endregion

        #region Public methods

        public bool Execute(UpdateResourceSetParameter udpateResourceSetParameter)
        {
            if (udpateResourceSetParameter == null)
            {
                throw new ArgumentNullException(nameof(udpateResourceSetParameter));
            }

            if (_resourceSetRepository.GetResourceSetById(udpateResourceSetParameter.Id) == null)
            {
                return false;
            }

            var resourceSet = new ResourceSet
            {
                Id = udpateResourceSetParameter.Id,
                Name = udpateResourceSetParameter.Name,
                Uri = udpateResourceSetParameter.Uri,
                Type = udpateResourceSetParameter.Type,
                Scopes = udpateResourceSetParameter.Scopes,
                IconUri = udpateResourceSetParameter.IconUri
            };

            _resourceSetParameterValidator.CheckResourceSetParameter(resourceSet);
            var result = _resourceSetRepository.UpdateResource(resourceSet);
            if (result == null)
            {
                throw new BaseUmaException(
                    ErrorCodes.InternalError,
                    string.Format(ErrorDescriptions.TheResourceSetCannotBeUpdated, resourceSet.Id));
            }

            return true;
        }

        #endregion
    }
}
