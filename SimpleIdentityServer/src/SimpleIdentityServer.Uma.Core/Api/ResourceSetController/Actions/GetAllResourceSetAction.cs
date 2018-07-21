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
using SimpleIdentityServer.Uma.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions
{
    internal interface IGetAllResourceSetAction
    {
        Task<IEnumerable<string>> Execute();
    }

    internal class GetAllResourceSetAction : IGetAllResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        public GetAllResourceSetAction(
            IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
        }

        public async Task<IEnumerable<string>> Execute()
        {
            var resourceSets = await _resourceSetRepository.GetAll().ConfigureAwait(false);
            if (resourceSets == null)
            {
                throw new BaseUmaException(
                    ErrorCodes.InternalError,
                    ErrorDescriptions.TheResourceSetsCannotBeRetrieved);
            }

            return resourceSets.Select(r => r.Id);
        }
    }
}
