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

using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Core.Common
{
    public interface IGenerateClientFromRegistrationRequest
    {
        Models.Client Execute(RegistrationParameter registrationParameter);
    }

    public class GenerateClientFromRegistrationRequest : IGenerateClientFromRegistrationRequest
    {
        private readonly IRegistrationParameterValidator _registrationParameterValidator;
        private readonly IJsonWebKeyConverter _jsonWebKeyConverter;

        public GenerateClientFromRegistrationRequest(
            IRegistrationParameterValidator registrationParameterValidator,
            IJsonWebKeyConverter jsonWebKeyConverter)
        {
            _registrationParameterValidator = registrationParameterValidator;
            _jsonWebKeyConverter = jsonWebKeyConverter;
        }
        

        public Models.Client Execute(RegistrationParameter registrationParameter)
        {
            if (registrationParameter == null)
            {
                throw new ArgumentNullException(nameof(registrationParameter));
            }
            
            // Validate the parameters
            _registrationParameterValidator.Validate(registrationParameter);
            // Generate the client
            var client = new Models.Client
            {
                RedirectionUrls = registrationParameter.RedirectUris,
                PostLogoutRedirectUris = registrationParameter.PostLogoutRedirectUris,
                Contacts = registrationParameter.Contacts,
                // TODO : should support different languages for the client_name
                ClientName = registrationParameter.ClientName,
                ClientUri = registrationParameter.ClientUri,
                PolicyUri = registrationParameter.PolicyUri,
                TosUri = registrationParameter.TosUri,
                JwksUri = registrationParameter.JwksUri,
                SectorIdentifierUri = registrationParameter.SectorIdentifierUri,
                // TODO : should support both subject types
                SubjectType = Constants.SubjectTypeNames.Public,
                DefaultMaxAge = registrationParameter.DefaultMaxAge,
                DefaultAcrValues = registrationParameter.DefaultAcrValues,
                RequireAuthTime = registrationParameter.RequireAuthTime,
                InitiateLoginUri = registrationParameter.InitiateLoginUri,
                RequestUris = registrationParameter.RequestUris,
                LogoUri = registrationParameter.LogoUri,
                ScimProfile = registrationParameter.ScimProfile
            };

            // If omitted then the default value is authorization code response type
            if (registrationParameter.ResponseTypes == null ||
                !registrationParameter.ResponseTypes.Any())
            {
                client.ResponseTypes = new List<ResponseType>
                {
                    ResponseType.code
                };
            }
            else
            {
                client.ResponseTypes = registrationParameter.ResponseTypes;
            }

            // If omitted then the default value is authorization code grant type
            if (registrationParameter.GrantTypes == null ||
                !registrationParameter.GrantTypes.Any())
            {
                client.GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code
                };
            }
            else
            {
                client.GrantTypes = registrationParameter.GrantTypes;
            }

            client.ApplicationType = registrationParameter.ApplicationType == null ? ApplicationTypes.web
                : registrationParameter.ApplicationType.Value;

            if (registrationParameter.Jwks != null)
            {
                var jsonWebKeys = _jsonWebKeyConverter.ExtractSerializedKeys(registrationParameter.Jwks);
                if (jsonWebKeys != null &&
                    jsonWebKeys.Any())
                {
                    client.JsonWebKeys = jsonWebKeys.ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(registrationParameter.IdTokenSignedResponseAlg) &&
                Constants.Supported.SupportedJwsAlgs.Contains(registrationParameter.IdTokenSignedResponseAlg))
            {
                client.IdTokenSignedResponseAlg = registrationParameter.IdTokenSignedResponseAlg;
            }
            else
            {
                client.IdTokenSignedResponseAlg = Jwt.Constants.JwsAlgNames.RS256;
            }

            if (!string.IsNullOrWhiteSpace(registrationParameter.IdTokenEncryptedResponseAlg) &&
                Constants.Supported.SupportedJweAlgs.Contains(registrationParameter.IdTokenEncryptedResponseAlg))
            {
                client.IdTokenEncryptedResponseAlg = registrationParameter.IdTokenEncryptedResponseAlg;
            }
            else
            {
                client.IdTokenEncryptedResponseAlg = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(client.IdTokenEncryptedResponseAlg))
            {
                if (!string.IsNullOrWhiteSpace(registrationParameter.IdTokenEncryptedResponseEnc) &&
                    Constants.Supported.SupportedJweEncs.Contains(registrationParameter.IdTokenEncryptedResponseEnc))
                {
                    client.IdTokenEncryptedResponseEnc = registrationParameter.IdTokenEncryptedResponseEnc;
                }
                else
                {
                    client.IdTokenEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256;
                }
            }

            if (!string.IsNullOrWhiteSpace(registrationParameter.UserInfoSignedResponseAlg) &&
                Constants.Supported.SupportedJwsAlgs.Contains(registrationParameter.UserInfoSignedResponseAlg))
            {
                client.UserInfoSignedResponseAlg = registrationParameter.UserInfoSignedResponseAlg;
            }
            else
            {
                client.UserInfoSignedResponseAlg = Jwt.Constants.JwsAlgNames.NONE;
            }

            if (!string.IsNullOrWhiteSpace(registrationParameter.UserInfoEncryptedResponseAlg) &&
                Constants.Supported.SupportedJweAlgs.Contains(registrationParameter.UserInfoEncryptedResponseAlg))
            {
                client.UserInfoEncryptedResponseAlg = registrationParameter.UserInfoEncryptedResponseAlg;
            }
            else
            {
                client.UserInfoEncryptedResponseAlg = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(client.UserInfoEncryptedResponseAlg))
            {
                if (!string.IsNullOrWhiteSpace(registrationParameter.UserInfoEncryptedResponseEnc) &&
                    Constants.Supported.SupportedJweEncs.Contains(registrationParameter.UserInfoEncryptedResponseEnc))
                {
                    client.UserInfoEncryptedResponseEnc = registrationParameter.UserInfoEncryptedResponseEnc;
                }
                else
                {
                    client.UserInfoEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256;
                }
            }

            if (!string.IsNullOrWhiteSpace(registrationParameter.RequestObjectSigningAlg) &&
                Constants.Supported.SupportedJwsAlgs.Contains(registrationParameter.RequestObjectSigningAlg))
            {
                client.RequestObjectSigningAlg = registrationParameter.RequestObjectSigningAlg;
            }
            else
            {
                client.RequestObjectSigningAlg = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(registrationParameter.RequestObjectEncryptionAlg) &&
                Constants.Supported.SupportedJweAlgs.Contains(registrationParameter.RequestObjectEncryptionAlg))
            {
                client.RequestObjectEncryptionAlg = registrationParameter.RequestObjectEncryptionAlg;
            }
            else
            {
                client.RequestObjectEncryptionAlg = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(client.RequestObjectEncryptionAlg))
            {
                if (!string.IsNullOrWhiteSpace(registrationParameter.RequestObjectEncryptionEnc) &&
                    Constants.Supported.SupportedJweEncs.Contains(registrationParameter.RequestObjectEncryptionEnc))
                {
                    client.RequestObjectEncryptionEnc = registrationParameter.RequestObjectEncryptionEnc;
                }
                else
                {
                    client.RequestObjectEncryptionEnc = Jwt.Constants.JweEncNames.A128CBC_HS256;
                }
            }

            TokenEndPointAuthenticationMethods tokenEndPointAuthenticationMethod;
            if (string.IsNullOrWhiteSpace(registrationParameter.TokenEndPointAuthMethod) ||
                !Enum.TryParse(registrationParameter.TokenEndPointAuthMethod, out tokenEndPointAuthenticationMethod))
            {
                tokenEndPointAuthenticationMethod = TokenEndPointAuthenticationMethods.client_secret_basic;
            }

            client.TokenEndPointAuthMethod = tokenEndPointAuthenticationMethod;

            if (!string.IsNullOrWhiteSpace(registrationParameter.TokenEndPointAuthSigningAlg) &&
                Constants.Supported.SupportedJwsAlgs.Contains(registrationParameter.TokenEndPointAuthSigningAlg))
            {
                client.TokenEndPointAuthSigningAlg = registrationParameter.TokenEndPointAuthSigningAlg;
            }
            else
            {
                client.TokenEndPointAuthSigningAlg = string.Empty;
            }

            return client;
        }
    }
}
