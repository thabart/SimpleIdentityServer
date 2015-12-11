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
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IAuthorizationCodeGrantTypeParameterTokenEdpValidator
    {
        void Validate(AuthorizationCodeGrantTypeParameter parameter);
    }

    public class AuthorizationCodeGrantTypeParameterTokenEdpValidator : IAuthorizationCodeGrantTypeParameterTokenEdpValidator
    {
        public void Validate(AuthorizationCodeGrantTypeParameter parameter)
        {
            /*
            if (string.IsNullOrWhiteSpace(parameter.ClientId))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "client_id"));
            }
            */

            if (string.IsNullOrWhiteSpace(parameter.Code))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "code"));
            }

            // With this instruction
            // The redirect_uri is considered well-formed according to the RFC-3986
            var redirectUrlIsCorrect = Uri.IsWellFormedUriString(parameter.RedirectUri, UriKind.Absolute);
            if (!redirectUrlIsCorrect)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestUriCode,
                    ErrorDescriptions.TheRedirectionUriIsNotWellFormed);
            }
        }
    }
}
