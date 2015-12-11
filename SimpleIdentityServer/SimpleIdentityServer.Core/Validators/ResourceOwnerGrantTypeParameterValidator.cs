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

namespace SimpleIdentityServer.Core.Validators
{
    public interface IResourceOwnerGrantTypeParameterValidator
    {
        void Validate(ResourceOwnerGrantTypeParameter parameter);
    }

    public sealed class ResourceOwnerGrantTypeParameterValidator : IResourceOwnerGrantTypeParameterValidator
    {
        public void Validate(ResourceOwnerGrantTypeParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.ClientId))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "clientId"));
            }

            if (string.IsNullOrWhiteSpace(parameter.UserName))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "username"));
            }

            if (string.IsNullOrWhiteSpace(parameter.Password))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "password"));
            }
        }
    }
}
