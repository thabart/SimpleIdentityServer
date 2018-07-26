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

using System;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Core.Validators;
using SimpleIdentityServer.Uma.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions
{
    internal interface IAddResourceSetAction
    {
        Task<string> Execute(AddResouceSetParameter addResourceSetParameter);
    }

    internal class AddResourceSetAction : IAddResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IResourceSetParameterValidator _resourceSetParameterValidator;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public AddResourceSetAction(
            IResourceSetRepository resourceSetRepository,
            IResourceSetParameterValidator resourceSetParameterValidator,
            IUmaServerEventSource umaServerEventSource)
        {
            _resourceSetRepository = resourceSetRepository;
            _resourceSetParameterValidator = resourceSetParameterValidator;
            _umaServerEventSource = umaServerEventSource;
        }
    
        public async Task<string> Execute(AddResouceSetParameter addResourceSetParameter)
        {
            var json = addResourceSetParameter == null ? string.Empty : JsonConvert.SerializeObject(addResourceSetParameter);
            _umaServerEventSource.StartToAddResourceSet(json);
            if (addResourceSetParameter == null)
            {
                throw new ArgumentNullException(nameof(addResourceSetParameter));
            }
            
            var resourceSet = new ResourceSet
            {
                Id = Guid.NewGuid().ToString(),
                Name = addResourceSetParameter.Name,
                Uri = addResourceSetParameter.Uri,
                Type = addResourceSetParameter.Type,
                Scopes = addResourceSetParameter.Scopes,
                IconUri = addResourceSetParameter.IconUri
            };

            _resourceSetParameterValidator.CheckResourceSetParameter(resourceSet);
            if (!await _resourceSetRepository.Insert(resourceSet))
            {
                throw new BaseUmaException(ErrorCodes.InternalError, ErrorDescriptions.TheResourceSetCannotBeInserted);
            }

            _umaServerEventSource.FinishToAddResourceSet(JsonConvert.SerializeObject(resourceSet));
            return resourceSet.Id;
        }
    }
}