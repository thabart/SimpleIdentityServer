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

namespace SimpleIdentityServer.Uma.Core.Api.ScopeController.Actions
{
    internal interface IUpdateScopeAction
    {
        bool Execute(UpdateScopeParameter updateScopeParameter);
    }

    internal class UpdateScopeAction : IUpdateScopeAction
    {
        private readonly IScopeRepository _scopeRepository;

        private readonly IScopeParameterValidator _scopeParameterValidator;

        #region Constructor

        public UpdateScopeAction(
            IScopeRepository scopeRepository,
            IScopeParameterValidator scopeParameterValidator)
        {
            _scopeRepository = scopeRepository;
            _scopeParameterValidator = scopeParameterValidator;
        }

        #endregion

        #region Public methods


        public bool Execute(UpdateScopeParameter updateScopeParameter)
        {
            if (updateScopeParameter == null)
            {
                throw new ArgumentNullException(nameof(updateScopeParameter));
            }

            Scope scope = null;
            try
            {
                scope = _scopeRepository.GetScope(updateScopeParameter.Id);
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

            scope = new Scope
            {
                Id = updateScopeParameter.Id,
                IconUri = updateScopeParameter.IconUri,
                Name = updateScopeParameter.Name
            };
            _scopeParameterValidator.CheckScopeParameter(scope);

            try
            {
                _scopeRepository.UpdateScope(scope);
                return true;
            }
            catch (Exception ex)
            {
                throw new BaseUmaException(ErrorCodes.InternalError,
                    ErrorDescriptions.TheScopeCannotBeUpdated,
                    ex);
            }
        }

        #endregion
    }
}
