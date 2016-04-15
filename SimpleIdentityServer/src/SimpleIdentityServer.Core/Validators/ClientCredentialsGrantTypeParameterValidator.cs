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

using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using System;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IClientCredentialsGrantTypeParameterValidator
    {
        void Validate(ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter);
    }

    internal class ClientCredentialsGrantTypeParameterValidator : IClientCredentialsGrantTypeParameterValidator
    {
        #region Public methods

        public void Validate(ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter)
        {
            if (clientCredentialsGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(clientCredentialsGrantTypeParameter));
            }

            if (string.IsNullOrWhiteSpace(clientCredentialsGrantTypeParameter.Scope))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.ScopeName));
            }
        }

        #endregion
    }
}
