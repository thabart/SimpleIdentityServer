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
using System.Linq;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using System.Collections.Generic;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IRegistrationParameterValidator
    {
        void Validate(RegistrationParameter parameter);
    }

    public class RegistrationParameterValidator : IRegistrationParameterValidator
    {
        public void Validate(RegistrationParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            // Validate the redirection uris
            if (parameter.RedirectUris == null ||
                !parameter.RedirectUris.Any())
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRedirectUri,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.StandardRegistrationRequestParameterNames.RequestUris));
            }

            foreach (var redirectUri in parameter.RedirectUris)
            {
                var redirectUrlIsCorrect = Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute);
                if (!redirectUrlIsCorrect)
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidRedirectUri,
                        ErrorDescriptions.TheRedirectUriParameterIsNotValid);
                }
            }

            // If the response type is not defined then set to code
            if (parameter.ResponseTypes == null ||
                !parameter.ResponseTypes.Any())
            {
                parameter.ResponseTypes = new List<ResponseType>
                {
                    ResponseType.code
                };
            }

            // If the grant type is not defined then set to authorization_code
            if (parameter.GrantTypes == null ||
                !parameter.GrantTypes.Any())
            {
                parameter.GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code
                };
            }

            // If the application type is not defined then set to web
            if (parameter.ApplicationType == null)
            {
                parameter.ApplicationType = ApplicationTypes.web;
            }


        }
    }
}
