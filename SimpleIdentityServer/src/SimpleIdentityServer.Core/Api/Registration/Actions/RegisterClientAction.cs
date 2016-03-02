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
using System.Globalization;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Logging;
using System.Linq;
using System.Collections.Generic;
using SimpleIdentityServer.Core.Common;

namespace SimpleIdentityServer.Core.Api.Registration.Actions
{
    public interface IRegisterClientAction
    {
        RegistrationResponse Execute(RegistrationParameter registrationParameter);
    }

    public class RegisterClientAction : IRegisterClientAction
    {
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        private readonly IClientRepository _clientRepository;

        private readonly IGenerateClientFromRegistrationRequest _generateClientFromRegistrationRequest;

        public RegisterClientAction(
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IClientRepository clientRepository,
            IGenerateClientFromRegistrationRequest generateClientFromRegistrationRequest)
        {
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _clientRepository = clientRepository;
            _generateClientFromRegistrationRequest = generateClientFromRegistrationRequest;
        }

        public RegistrationResponse Execute(RegistrationParameter registrationParameter)
        {
            if (registrationParameter == null)
            {
                throw new ArgumentNullException("registrationParameter");
            }

            _simpleIdentityServerEventSource.StartRegistration(registrationParameter.ClientName);

            var client = _generateClientFromRegistrationRequest.Execute(registrationParameter);

            client.AllowedScopes = new List<Scope>
            {
                Constants.StandardScopes.OpenId,
                Constants.StandardScopes.ProfileScope,
                Constants.StandardScopes.Address,
                Constants.StandardScopes.Email,
                Constants.StandardScopes.Phone
            };

            var result = new RegistrationResponse
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientSecretExpiresAt = 0,
                ClientIdIssuedAt = DateTime.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                ApplicationType = Enum.GetName(typeof(ApplicationTypes), client.ApplicationType),
                ClientUri = GetDefaultValue(client.ClientUri),
                ClientName = GetDefaultValue(client.ClientName),
                Contacts = GetDefaultValues(client.Contacts).ToArray(),
                DefaultAcrValues = GetDefaultValue(client.DefaultAcrValues),
                GrantTypes = client.GrantTypes == null ?
                    new string[0] : 
                    client.GrantTypes.Select(g => Enum.GetName(typeof(GrantType), g)).ToArray(),
                DefaultMaxAge = client.DefaultMaxAge,
                IdTokenEncryptedResponseAlg = GetDefaultValue(client.IdTokenEncryptedResponseAlg),
                IdTokenEncryptedResponseEnc = GetDefaultValue(client.IdTokenEncryptedResponseEnc),
                JwksUri = GetDefaultValue(client.JwksUri),
                RequestObjectEncryptionAlg = GetDefaultValue(client.RequestObjectEncryptionAlg),
                RequestObjectEncryptionEnc = GetDefaultValue(client.RequestObjectEncryptionEnc),
                IdTokenSignedResponseAlg = GetDefaultValue(client.IdTokenSignedResponseAlg),
                LogoUri = GetDefaultValue(client.LogoUri),
                Jwks = registrationParameter.Jwks,
                RequireAuthTime = client.RequireAuthTime,
                InitiateLoginUri = GetDefaultValue(client.InitiateLoginUri),
                PolicyUri = GetDefaultValue(client.PolicyUri),
                RequestObjectSigningAlg = GetDefaultValue(client.RequestObjectSigningAlg),
                UserInfoEncryptedResponseAlg = GetDefaultValue(client.UserInfoEncryptedResponseAlg),
                UserInfoEncryptedResponseEnc = GetDefaultValue(client.UserInfoEncryptedResponseEnc),
                UserInfoSignedResponseAlg = GetDefaultValue(client.UserInfoSignedResponseAlg),
                TosUri = GetDefaultValue(client.TosUri),
                SectorIdentifierUri = GetDefaultValue(client.SectorIdentifierUri),
                SubjectType = GetDefaultValue(client.SubjectType),
                ResponseTypes = client.ResponseTypes == null ? new string[0] :
                    client.ResponseTypes.Select(r => Enum.GetName(typeof(ResponseType), r)).ToArray(),
                RequestUris = GetDefaultValues(client.RequestUris).ToList(),
                RedirectUris = GetDefaultValues(client.RedirectionUrls).ToArray(),
                TokenEndPointAuthSigningAlg = GetDefaultValue(client.TokenEndPointAuthSigningAlg),
                TokenEndPointAuthMethod = Enum.GetName(typeof(TokenEndPointAuthenticationMethods), client.TokenEndPointAuthMethod)
            };

            if (client.TokenEndPointAuthMethod != TokenEndPointAuthenticationMethods.private_key_jwt)
            {
                result.ClientSecret = Guid.NewGuid().ToString();
                client.ClientSecret = result.ClientSecret;
            }

            client.ClientId = result.ClientId;
            _clientRepository.InsertClient(client);

            _simpleIdentityServerEventSource.EndRegistration(result.ClientId, 
                client.ClientName);

            return result;
        }

        private static string GetDefaultValue(string value)
        {
            return value == null ? string.Empty : value;
        }

        private static IEnumerable<string> GetDefaultValues(IEnumerable<string> value)
        {
            return value == null ? new string[0] : value;
        }
    }
}
