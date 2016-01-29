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
using System.Net.Http;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Factories;
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
        private readonly IHttpClientFactory _httpClientFactory;

        public RegistrationParameterValidator(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void Validate(RegistrationParameter parameter)
        {
            const string localhost = "localhost";
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
                if (!CheckUriIsWellFormed(redirectUri))
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidRedirectUri,
                        ErrorDescriptions.TheRedirectUriParameterIsNotValid);
                }

                var uri = new Uri(redirectUri);
                if (!string.IsNullOrWhiteSpace(uri.Fragment))
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidRedirectUri,
                        ErrorDescriptions.TheRedirectUriContainsAFragment);
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

            // Check the parameters when the application type is web
            if (parameter.ApplicationType == ApplicationTypes.web)
            {
                foreach(var redirectUri in parameter.RedirectUris)
                {
                    var uri = new Uri(redirectUri);
                    if (uri.Scheme != Uri.UriSchemeHttps ||
                        string.Compare(uri.Host, localhost, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        throw new IdentityServerException(
                            ErrorCodes.InvalidRedirectUri,
                            ErrorDescriptions.TheRedirectUriParameterIsNotValid);
                    }
                }
            }

            // Check the parameters when the application type is native
            if (parameter.ApplicationType == ApplicationTypes.native)
            {
                foreach(var redirectUri in parameter.RedirectUris)
                {
                    var uri = new Uri(redirectUri);
                    if (string.Compare(uri.Host, localhost, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        throw new IdentityServerException(
                            ErrorCodes.InvalidRedirectUri,
                            ErrorDescriptions.TheRedirectUriParameterIsNotValid);
                    }
                }
            }

            ValidateNotMandatoryUri(parameter.LogoUri, Constants.StandardRegistrationRequestParameterNames.LogoUri);
            ValidateNotMandatoryUri(parameter.ClientUri, Constants.StandardRegistrationRequestParameterNames.ClientUri);
            ValidateNotMandatoryUri(parameter.TosUri, Constants.StandardRegistrationRequestParameterNames.TosUri);
            ValidateNotMandatoryUri(parameter.JwksUri, Constants.StandardRegistrationRequestParameterNames.JwksUri);

            if (parameter.Jwks != null)
            {
                if (!string.IsNullOrWhiteSpace(parameter.JwksUri))
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidClientMetaData,
                        ErrorDescriptions.TheJwksParameterCannotBeSetBecauseJwksUrlIsUsed);
                }
            }

            ValidateNotMandatoryUri(parameter.SectorIdentifierUri,
                Constants.StandardRegistrationRequestParameterNames.SectoreIdentifierUri, true);

            // Based on the RFC : http://openid.net/specs/openid-connect-registration-1_0.html#SectorIdentifierValidation validate the sector_identifier_uri
            if (!string.IsNullOrWhiteSpace(parameter.SectorIdentifierUri))
            {
                var sectorIdentifierUris = GetSectorIdentifierUris(parameter.SectorIdentifierUri);
                foreach (var sectorIdentifierUri in sectorIdentifierUris)
                {
                    if (!parameter.RedirectUris.Contains(sectorIdentifierUri))
                    {
                        throw new IdentityServerException(
                            ErrorCodes.InvalidClientMetaData,
                            ErrorDescriptions.OneOrMoreSectorIdentifierUriIsNotARedirectUri);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(parameter.IdTokenEncryptedResponseEnc) &&
                Jwt.Constants.MappingNameToJweEncEnum.Keys.Contains(parameter.IdTokenEncryptedResponseEnc))
            {
                if (string.IsNullOrWhiteSpace(parameter.IdTokenEncryptedResponseAlg) ||
                    !Jwt.Constants.MappingNameToJweAlgEnum.ContainsKey(parameter.IdTokenEncryptedResponseAlg))
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidClientMetaData,
                        ErrorDescriptions.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
                }
            }

            if (!string.IsNullOrWhiteSpace(parameter.UserInfoEncryptedResponseEnc) &&
                Jwt.Constants.MappingNameToJweEncEnum.Keys.Contains(parameter.UserInfoEncryptedResponseEnc))
            {
                if (string.IsNullOrWhiteSpace(parameter.UserInfoEncryptedResponseAlg) ||
                    !Jwt.Constants.MappingNameToJweAlgEnum.ContainsKey(parameter.UserInfoEncryptedResponseAlg))
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidClientMetaData,
                        ErrorDescriptions.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified);
                }
            }

            if (!string.IsNullOrWhiteSpace(parameter.RequestObjectEncryptionEnc) &&
                Jwt.Constants.MappingNameToJweEncEnum.Keys.Contains(parameter.RequestObjectEncryptionEnc))
            {
                if (string.IsNullOrWhiteSpace(parameter.RequestObjectEncryptionAlg) ||
                    !Jwt.Constants.MappingNameToJweAlgEnum.ContainsKey(parameter.RequestObjectEncryptionAlg))
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidClientMetaData,
                        ErrorDescriptions.TheParameterRequestObjectEncryptionAlgMustBeSpecified);
                }
            }

            ValidateNotMandatoryUri(parameter.InitiateLoginUri,
                Constants.StandardRegistrationRequestParameterNames.InitiateLoginUri, true);

            if (parameter.RequestUris != null &&
                parameter.RequestUris.Any())
            {
                foreach (var requestUri in parameter.RequestUris)
                {
                    if (!CheckUriIsWellFormed(requestUri))
                    {
                        throw new IdentityServerException(
                            ErrorCodes.InvalidClientMetaData,
                            ErrorDescriptions.OneOfTheRequestUriIsNotValid);
                    }
                }
            }
        }

        private List<string> GetSectorIdentifierUris(string sectorIdentifierUri)
        {
            var httpClient = _httpClientFactory.GetHttpClient();
            try
            {
                var response = httpClient.GetAsync(sectorIdentifierUri).Result;
                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadAsStringAsync().Result;
                return result.DeserializeWithJavascript<List<string>>();
            }
            catch (Exception)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidClientMetaData,
                    ErrorDescriptions.TheSectorIdentifierUrisCannotBeRetrieved);
            }
        }

        private static void ValidateNotMandatoryUri(string uri,
            string parameterName,
            bool checkSchemeIsHttps = false)
        {
            // Check the parameter logo_uri is valid
            if (!string.IsNullOrWhiteSpace(uri))
            {
                if (!CheckUriIsWellFormed(uri))
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidClientMetaData,
                        string.Format(ErrorDescriptions.ParameterIsNotCorrect, parameterName));
                }

                if (checkSchemeIsHttps)
                {
                    var u = new Uri(uri);
                    if (u.Scheme != Uri.UriSchemeHttps)
                    {
                        throw new IdentityServerException(
                            ErrorCodes.InvalidClientMetaData,
                            string.Format(ErrorDescriptions.ParameterIsNotCorrect, parameterName));
                    }
                }
            }
        }

        private static bool CheckUriIsWellFormed(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.Absolute);
        }
    }
}
