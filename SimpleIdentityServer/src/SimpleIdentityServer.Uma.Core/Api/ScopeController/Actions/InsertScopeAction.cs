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

using Newtonsoft.Json;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Core.Validators;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.ScopeController.Actions
{
    internal interface IInsertScopeAction
    {
        Task<bool> Execute(AddScopeParameter addScopeParameter);
    }

    internal class InsertScopeAction : IInsertScopeAction
    {
        private readonly IScopeRepository _scopeRepository;
        private readonly IScopeParameterValidator _scopeParameterValidator;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public InsertScopeAction(
            IScopeRepository scopeRepository,
            IScopeParameterValidator scopeParameterValidator,
            IUmaServerEventSource umaServerEventSource)
        {
            _scopeRepository = scopeRepository;
            _scopeParameterValidator = scopeParameterValidator;
            _umaServerEventSource = umaServerEventSource;
        }

        public async Task<bool> Execute(AddScopeParameter addScopeParameter)
        {
            var json = addScopeParameter == null ? string.Empty : JsonConvert.SerializeObject(addScopeParameter);
            _umaServerEventSource.StartToAddScope(json);
            if (addScopeParameter == null)
            {
                throw new ArgumentNullException(nameof(addScopeParameter));
            }

            Scope scope = null;
            try
            {
                scope = await _scopeRepository.Get(addScopeParameter.Id);
            }
            catch (Exception ex)
            {
                throw new BaseUmaException(ErrorCodes.InternalError,
                    ErrorDescriptions.TheScopeCannotBeRetrieved,
                    ex);
            }

            if (scope != null)
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheScopeAlreadyExists, addScopeParameter.Id));
            }

            scope = new Scope
            {
                Id = addScopeParameter.Id,
                IconUri = addScopeParameter.IconUri,
                Name = addScopeParameter.Name
            };
            _scopeParameterValidator.CheckScopeParameter(scope);
            
            try
            {
                await _scopeRepository.Insert(scope);
                _umaServerEventSource.FinishToAddScope(json);
                return true;
            }
            catch(Exception ex)
            {
                throw new BaseUmaException(ErrorCodes.InternalError,
                    ErrorDescriptions.TheScopeCannotBeInserted,
                    ex);
            }
        }
    }
}
