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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.ScopeController.Actions
{
    internal interface IGetScopesAction
    {
        Task<IEnumerable<string>> Execute();
    }

    internal class GetScopesAction : IGetScopesAction
    {
        private readonly IScopeRepository _scopeRepository;

        public GetScopesAction(IScopeRepository scopeRepository)
        {
            _scopeRepository = scopeRepository;
        }

        public async Task<IEnumerable<string>> Execute()
        {
            try
            {
                var scopes = await _scopeRepository.GetAll();
                return scopes.Select(s => s.Id);
            }
            catch (Exception ex)
            {
                throw new BaseUmaException(ErrorCodes.InternalError,
                    ErrorDescriptions.TheScopesCannotBeRetrieved,
                    ex);
            }
        }
    }
}
