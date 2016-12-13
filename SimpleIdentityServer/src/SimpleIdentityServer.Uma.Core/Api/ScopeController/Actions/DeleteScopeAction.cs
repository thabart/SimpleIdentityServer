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
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.ScopeController.Actions
{
    internal interface IDeleteScopeAction
    {
        Task<bool> Execute(string scopeId);
    }

    internal class DeleteScopeAction : IDeleteScopeAction
    {
        private readonly IScopeRepository _scopeRepository;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public DeleteScopeAction(
            IScopeRepository scopeRepository,
            IUmaServerEventSource umaServerEventSource)
        {
            _scopeRepository = scopeRepository;
            _umaServerEventSource = umaServerEventSource;
        }

        public async Task<bool> Execute(string scopeId)
        {
            _umaServerEventSource.StartToRemoveScope(scopeId);
            if (string.IsNullOrWhiteSpace(scopeId))
            {
                throw new ArgumentNullException(nameof(scopeId));
            }

            Scope scope = null;
            try
            {
                scope = await _scopeRepository.Get(scopeId);
            }
            catch (Exception ex)
            {
                throw new BaseUmaException(ErrorCodes.InternalError,
                    ErrorDescriptions.TheScopeCannotBeRetrieved,
                    ex);
            }

            if (scope == null)
            {
                return false;
            }

            try
            {
                await _scopeRepository.Delete(scopeId);
                _umaServerEventSource.FinishToRemoveScope(scopeId);
                return true;
            }
            catch (Exception ex)
            {
                throw new BaseUmaException(ErrorCodes.InternalError,
                    ErrorDescriptions.TheScopeCannotBeRemoved,
                    ex);
            }
        }
    }
}
