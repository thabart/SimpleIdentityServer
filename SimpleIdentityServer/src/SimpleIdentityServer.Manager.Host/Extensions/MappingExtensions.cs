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

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Manager.Core.Parameters;
using SimpleIdentityServer.Manager.Core.Results;
using SimpleIdentityServer.Manager.Host.DTOs.Requests;
using SimpleIdentityServer.Manager.Host.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Manager.Host.Extensions
{
    public static class MappingExtensions
    {
        #region To parameters
    
        public static GetJwsParameter ToParameter(this GetJwsRequest getJwsRequest)
        {
            return new GetJwsParameter
            {
                Jws = getJwsRequest.Jws,
                Url = getJwsRequest.Url
            };
        }

        public static GetJweParameter ToParameter(this GetJweRequest getJweRequest)
        {
            return new GetJweParameter
            {
                Jwe = getJweRequest.Jwe,
                Password = getJweRequest.Password,
                Url = getJweRequest.Url
            };
        }

        public static CreateJweParameter ToParameter(this CreateJweRequest createJweRequest)
        {
            return new CreateJweParameter
            {
                Alg = createJweRequest.Alg,
                Enc = createJweRequest.Enc,
                Jws = createJweRequest.Jws,
                Kid = createJweRequest.Kid,
                Password = createJweRequest.Password,
                Url = createJweRequest.Url
            };
        }

        public static CreateJwsParameter ToParameter(this CreateJwsRequest createJwsRequest)
        {
            return new CreateJwsParameter
            {
                Alg = createJwsRequest.Alg,
                Kid = createJwsRequest.Kid,
                Url = createJwsRequest.Url,
                Payload = createJwsRequest.Payload
            };
        }

        public static UpdateClientParameter ToParameter(this UpdateClientRequest updateClientRequest)
        {
            return new UpdateClientParameter
            {
                ApplicationType = updateClientRequest.ApplicationType,
                ClientId = updateClientRequest.ClientId,
                ClientName = updateClientRequest.ClientName,
                ClientUri = updateClientRequest.ClientUri,
                Contacts = updateClientRequest.Contacts,
                DefaultAcrValues = updateClientRequest.DefaultAcrValues,
                DefaultMaxAge = updateClientRequest.DefaultMaxAge,
                GrantTypes = updateClientRequest.GrantTypes,
                IdTokenEncryptedResponseAlg = updateClientRequest.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = updateClientRequest.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = updateClientRequest.IdTokenSignedResponseAlg,
                InitiateLoginUri = updateClientRequest.InitiateLoginUri,
                Jwks = updateClientRequest.Jwks,
                JwksUri = updateClientRequest.JwksUri,
                LogoUri = updateClientRequest.LogoUri,
                PolicyUri = updateClientRequest.PolicyUri,
                RedirectUris = updateClientRequest.RedirectUris,
                RequestObjectEncryptionAlg = updateClientRequest.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = updateClientRequest.RequestObjectEncryptionEnc,
                RequestObjectSigningAlg = updateClientRequest.RequestObjectSigningAlg,
                RequestUris = updateClientRequest.RequestUris,
                RequireAuthTime = updateClientRequest.RequireAuthTime,
                ResponseTypes = updateClientRequest.ResponseTypes,
                SectorIdentifierUri = updateClientRequest.SectorIdentifierUri,
                SubjectType = updateClientRequest.SubjectType,
                TokenEndPointAuthMethod = updateClientRequest.TokenEndPointAuthMethod,
                TokenEndPointAuthSigningAlg = updateClientRequest.TokenEndPointAuthSigningAlg,
                TosUri = updateClientRequest.TosUri,
                UserInfoEncryptedResponseAlg = updateClientRequest.UserInfoEncryptedResponseAlg,
                UserInfoEncryptedResponseEnc = updateClientRequest.UserInfoEncryptedResponseEnc,
                UserInfoSignedResponseAlg = updateClientRequest.UserInfoSignedResponseAlg
            };
        }

        #endregion

        #region To DTOs

        public static JwsInformationResponse ToDto(this JwsInformationResult jwsInformationResult)
        {
            return new JwsInformationResponse
            {
                Header = jwsInformationResult.Header,
                JsonWebKey = jwsInformationResult.JsonWebKey,
                Payload = jwsInformationResult.Payload
            };
        }

        public static JweInformationResponse ToDto(this JweInformationResult jweInformationResult)
        {
            return new JweInformationResponse
            {
                IsContentJws = jweInformationResult.IsContentJws,
                Content = jweInformationResult.Content
            };
        }

        public static ClientInformationResponse ToDto(this Client client)
        {
            return new ClientInformationResponse
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                LogoUri = client.LogoUri
            };
        }

        public static ClientResponse ToClientResponseDto(this Client client)
        {
            return new ClientResponse
            {
                AllowedScopes = client.AllowedScopes == null ? new List<string>() : client.AllowedScopes.Select(c => c.Name).ToList(),
                ApplicationType = Enum.GetName(typeof(ApplicationTypes), client.ApplicationType),
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                ClientSecret = client.ClientSecret,
                ClientUri = client.ClientUri,
                Contacts = client.Contacts,
                DefaultAcrValues = client.DefaultAcrValues,
                DefaultMaxAge = client.DefaultMaxAge,
                GrantTypes = client.GrantTypes == null ? new List<string>() : client.GrantTypes.Select(g => Enum.GetName(typeof(GrantType), g)).ToList(),
                IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg,
                InitiateLoginUri = client.InitiateLoginUri,
                JsonWebKeys = client.JsonWebKeys,
                JwksUri = client.JwksUri,
                LogoUri = client.LogoUri,
                PolicyUri = client.PolicyUri,
                RedirectUris = client.RedirectionUrls,
                RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc,
                RequestObjectSigningAlg = client.RequestObjectSigningAlg,
                RequestUris = client.RequestUris,
                RequireAuthTime = client.RequireAuthTime,
                ResponseTypes = client.ResponseTypes == null ? new List<string>() : client.ResponseTypes.Select(g => Enum.GetName(typeof(ResponseType), g)).ToList(),
                SectorIdentifierUri = client.SectorIdentifierUri,
                SubjectType = client.SubjectType,
                TokenEndPointAuthMethod = Enum.GetName(typeof(TokenEndPointAuthenticationMethods), client.TokenEndPointAuthMethod),
                TokenEndPointAuthSigningAlg = client.TokenEndPointAuthSigningAlg,
                UserInfoEncryptedResponseAlg = client.UserInfoEncryptedResponseAlg,
                UserInfoEncryptedResponseEnc = client.UserInfoEncryptedResponseEnc,
                UserInfoSignedResponseAlg = client.UserInfoSignedResponseAlg,
                TosUri = client.TosUri
            };
        }

        public static ScopeResponse ToDto(this Scope scope)
        {
            return new ScopeResponse
            {
                Claims = scope.Claims,
                Description = scope.Description,
                IsDisplayedInConsent = scope.IsDisplayedInConsent,
                IsExposed = scope.IsExposed,
                IsOpenIdScope = scope.IsOpenIdScope,
                Name = scope.Name,
                Type = scope.Type
            };
        }

        #endregion

        #region To List of DTOs

        public static List<ClientInformationResponse> ToDtos(this List<Client> clients)
        {
            return clients.Select(c => c.ToDto()).ToList();
        }

        public static List<ScopeResponse> ToDtos(this List<Scope> scopes)
        {
            return scopes.Select(s => s.ToDto()).ToList();
        }

        #endregion
    }
}
