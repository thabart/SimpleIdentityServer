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
using System;

namespace SimpleIdentityServer.Uma.Core.Validators
{
    public interface IScopeParameterValidator
    {
        void CheckScopeParameter(Scope scope);
    }

    internal class ScopeParameterValidator : IScopeParameterValidator
    {
        #region Public methods

        public void CheckScopeParameter(Scope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (string.IsNullOrWhiteSpace(scope.Id))
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "id"));
            }

            if (string.IsNullOrWhiteSpace(scope.Name))
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "name"));
            }
            
            if (!string.IsNullOrWhiteSpace(scope.IconUri) &&
                !Uri.IsWellFormedUriString(scope.IconUri, UriKind.Absolute))
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, scope.IconUri));
            }
        }

        #endregion
    }
}
