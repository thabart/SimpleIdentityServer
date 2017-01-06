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
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Manager.Core.Parameters;
using SimpleIdentityServer.Manager.Core.Results;
using SimpleIdentityServer.Manager.Host.DTOs.Requests;
using SimpleIdentityServer.Manager.Host.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SimpleIdentityServer.Manager.Host.Extensions
{
    public static class MappingExtensions
    {
        #region To parameters
    
        public static ResourceOwner ToParameter(this ResourceOwnerResponse request)
        {
            var claims = new List<Claim>();
            if (request.Claims != null)
            {
                claims = request.Claims.Select(s => new Claim(s.Key, s.Value)).ToList();
            }

            return new ResourceOwner
            {
                Id = request.Login,
                Password = request.Password,
                IsLocalAccount = request.IsLocalAccount,
                TwoFactorAuthentication = (SimpleIdentityServer.Core.Models.TwoFactorAuthentications)request.TwoFactorAuthentication,
                Claims = claims
            };
        }

        public static AddUserParameter ToParameter(this AddResourceOwnerRequest request)
        {
            return new AddUserParameter
            {
                Login = request.Subject,
                Password = request.Password
            };
        }

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

        public static Scope ToParameter(this ScopeResponse scopeResponse)
        {
            if (scopeResponse == null)
            {
                throw new ArgumentNullException(nameof(scopeResponse));
            }

            return new Scope
            {
                Description = scopeResponse.Description,
                IsDisplayedInConsent = scopeResponse.IsDisplayedInConsent,
                IsExposed = scopeResponse.IsExposed,
                IsOpenIdScope = scopeResponse.IsOpenIdScope,
                Name = scopeResponse.Name,
                Type = scopeResponse.Type,
                Claims = scopeResponse.Claims
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
                UserInfoSignedResponseAlg = updateClientRequest.UserInfoSignedResponseAlg,
                AllowedScopes = updateClientRequest.AllowedScopes == null ? new List<string>() : updateClientRequest.AllowedScopes
            };
        }

        public static RegistrationParameter ToParameter(this ClientResponse clientResponse)
        {
            var responseTypes = new List<ResponseType>();
            var redirectUris = clientResponse.RedirectUris == null
                ? new List<string>()
                : clientResponse.RedirectUris.ToList();
            var grantTypes = new List<GrantType>();
            ApplicationTypes? applicationType = null;
            if (clientResponse.ResponseTypes != null &&
                clientResponse.ResponseTypes.Any())
            {
                foreach (var responseType in clientResponse.ResponseTypes)
                {
                    var responseTypeSplitted = responseType.Split(' ');
                    foreach (var response in responseTypeSplitted)
                    {
                        ResponseType responseTypeEnum;
                        if (Enum.TryParse(response, out responseTypeEnum) &&
                            !responseTypes.Contains(responseTypeEnum))
                        {
                            responseTypes.Add(responseTypeEnum);
                        }
                    }
                }
            }

            if (clientResponse.GrantTypes != null &&
                clientResponse.GrantTypes.Any())
            {
                foreach (var grantType in clientResponse.GrantTypes)
                {
                    GrantType grantTypeEnum;
                    if (Enum.TryParse(grantType, out grantTypeEnum))
                    {
                        grantTypes.Add(grantTypeEnum);
                    }
                }
            }

            ApplicationTypes appTypeEnum;
            if (Enum.TryParse(clientResponse.ApplicationType, out appTypeEnum))
            {
                applicationType = appTypeEnum;
            }

            return new RegistrationParameter
            {
                ApplicationType = applicationType,
                ClientName = clientResponse.ClientName,
                ClientUri = clientResponse.ClientUri,
                Contacts = clientResponse.Contacts == null ? new List<string>() : clientResponse.Contacts.ToList(),
                DefaultAcrValues = clientResponse.DefaultAcrValues,
                DefaultMaxAge = clientResponse.DefaultMaxAge,
                GrantTypes = grantTypes,
                IdTokenEncryptedResponseAlg = clientResponse.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = clientResponse.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = clientResponse.IdTokenSignedResponseAlg,
                InitiateLoginUri = clientResponse.InitiateLoginUri,
                Jwks = clientResponse.Jwks,
                JwksUri = clientResponse.JwksUri,
                LogoUri = clientResponse.LogoUri,
                PolicyUri = clientResponse.PolicyUri,
                RedirectUris = redirectUris,
                RequestObjectEncryptionAlg = clientResponse.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = clientResponse.RequestObjectEncryptionEnc,
                RequestObjectSigningAlg = clientResponse.RequestObjectSigningAlg,
                RequestUris = clientResponse.RequestUris,
                RequireAuthTime = clientResponse.RequireAuthTime,
                ResponseTypes = responseTypes,
                SectorIdentifierUri = clientResponse.SectorIdentifierUri,
                SubjectType = clientResponse.SubjectType,
                TokenEndPointAuthMethod = clientResponse.TokenEndPointAuthMethod,
                TokenEndPointAuthSigningAlg = clientResponse.TokenEndPointAuthSigningAlg,
                TosUri = clientResponse.TosUri,
                UserInfoEncryptedResponseAlg = clientResponse.UserInfoEncryptedResponseAlg,
                UserInfoEncryptedResponseEnc = clientResponse.UserInfoEncryptedResponseEnc,
                UserInfoSignedResponseAlg = clientResponse.UserInfoSignedResponseAlg
            };
        }

        public static SimpleIdentityServer.Core.Models.Client ToModel(this ClientResponse clientResponse)
        {
            var responseTypes = new List<ResponseType>();
            var grantTypes = new List<GrantType>();
            var redirectUris = clientResponse.RedirectUris == null
                ? new List<string>()
                : clientResponse.RedirectUris.ToList();
            var scopes = clientResponse.AllowedScopes == null ? new List<Scope>() : clientResponse.AllowedScopes.Select(s => new Scope
            {
                Name = s
            }).ToList();
            ApplicationTypes? applicationType = null;
            if (clientResponse.ResponseTypes != null &&
                clientResponse.ResponseTypes.Any())
            {
                foreach (var responseType in clientResponse.ResponseTypes)
                {
                    var responseTypeSplitted = responseType.Split(' ');
                    foreach (var response in responseTypeSplitted)
                    {
                        ResponseType responseTypeEnum;
                        if (Enum.TryParse(response, out responseTypeEnum) &&
                            !responseTypes.Contains(responseTypeEnum))
                        {
                            responseTypes.Add(responseTypeEnum);
                        }
                    }
                }
            }

            if (clientResponse.GrantTypes != null &&
                clientResponse.GrantTypes.Any())
            {
                foreach (var grantType in clientResponse.GrantTypes)
                {
                    GrantType grantTypeEnum;
                    if (Enum.TryParse(grantType, out grantTypeEnum))
                    {
                        grantTypes.Add(grantTypeEnum);
                    }
                }
            }

            ApplicationTypes appTypeEnum;
            if (Enum.TryParse(clientResponse.ApplicationType, out appTypeEnum))
            {
                applicationType = appTypeEnum;
            }

            TokenEndPointAuthenticationMethods tokenEndPointAuthenticationMethod;
            if (!Enum.TryParse(clientResponse.TokenEndPointAuthMethod, out tokenEndPointAuthenticationMethod))
            {
                tokenEndPointAuthenticationMethod = TokenEndPointAuthenticationMethods.client_secret_basic;
            }

            return new SimpleIdentityServer.Core.Models.Client
            {
                AllowedScopes = scopes,
                GrantTypes = grantTypes,
                TokenEndPointAuthMethod = tokenEndPointAuthenticationMethod,
                ApplicationType = appTypeEnum,
                ResponseTypes = responseTypes,
                ClientId = clientResponse.ClientId,
                ClientName = clientResponse.ClientName,
                // ClientSecret = clientResponse.ClientSecret,
                ClientUri = clientResponse.ClientUri,
                Contacts = clientResponse.Contacts,
                DefaultAcrValues = clientResponse.DefaultAcrValues,
                DefaultMaxAge = clientResponse.DefaultMaxAge,
                IdTokenEncryptedResponseAlg = clientResponse.IdTokenEncryptedResponseAlg,
                IdTokenEncryptedResponseEnc = clientResponse.IdTokenEncryptedResponseEnc,
                IdTokenSignedResponseAlg = clientResponse.IdTokenSignedResponseAlg,
                InitiateLoginUri = clientResponse.InitiateLoginUri,
                JwksUri = clientResponse.JwksUri,
                LogoUri = clientResponse.LogoUri,
                PolicyUri = clientResponse.PolicyUri,
                UserInfoSignedResponseAlg = clientResponse.UserInfoSignedResponseAlg,
                UserInfoEncryptedResponseEnc = clientResponse.UserInfoEncryptedResponseEnc,
                UserInfoEncryptedResponseAlg = clientResponse.UserInfoEncryptedResponseAlg,
                TosUri = clientResponse.TosUri,
                TokenEndPointAuthSigningAlg = clientResponse.TokenEndPointAuthSigningAlg,
                SubjectType = clientResponse.SubjectType,
                SectorIdentifierUri = clientResponse.SectorIdentifierUri,
                RequireAuthTime = clientResponse.RequireAuthTime,
                RequestObjectSigningAlg = clientResponse.RequestObjectSigningAlg,
                RequestObjectEncryptionAlg = clientResponse.RequestObjectEncryptionAlg,
                RequestObjectEncryptionEnc = clientResponse.RequestObjectEncryptionEnc,
                RedirectionUrls = redirectUris,
                RequestUris = clientResponse.RequestUris
            };
        }

        public static ImportParameter ToParameter(this ExportResponse export)
        {
            if (export == null)
            {
                throw new ArgumentNullException(nameof(export));
            }


            return new ImportParameter
            {
                Clients = export.Clients == null ? null : export.Clients.Select(c => c.ToModel())
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

        public static ExportResponse ToDto(this ExportResult export)
        {
            if (export == null)
            {
                throw new ArgumentNullException(nameof(export));
            }

            return new ExportResponse
            {
                Clients = export.Clients == null ? null : export.Clients.Select(c => c.ToClientResponseDto())
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

        public static ClientInformationResponse ToDto(this SimpleIdentityServer.Core.Models.Client client)
        {
            return new ClientInformationResponse
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                LogoUri = client.LogoUri
            };
        }

        public static ClientResponse ToClientResponseDto(this SimpleIdentityServer.Core.Models.Client client)
        {
            string secret = string.Empty;
            if (client.Secrets != null)
            {
                var clientSecret = client.Secrets.FirstOrDefault(s => s.Type == ClientSecretTypes.SharedSecret);
                if (clientSecret != null)
                {
                    secret = clientSecret.Value;
                }
            }

            return new ClientResponse
            {
                AllowedScopes = client.AllowedScopes == null ? new List<string>() : client.AllowedScopes.Select(c => c.Name).ToList(),
                ApplicationType = Enum.GetName(typeof(ApplicationTypes), client.ApplicationType),
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                ClientSecret = secret,
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

        public static ResourceOwnerResponse ToDto(this ResourceOwner resourceOwner)
        {
            var claims = new List<KeyValuePair<string, string>>();
            if (resourceOwner.Claims != null)
            {
                claims = resourceOwner.Claims.Select(s => new KeyValuePair<string, string>(s.Type, s.Value)).ToList();
            }

            return new ResourceOwnerResponse
            {
                Login = resourceOwner.Id,
                Password = resourceOwner.Password,
                IsLocalAccount = resourceOwner.IsLocalAccount,
                Claims = claims,
                TwoFactorAuthentication = (DTOs.Responses.TwoFactorAuthentications)resourceOwner.TwoFactorAuthentication
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

        public static List<ClientInformationResponse> ToDtos(this IEnumerable<SimpleIdentityServer.Core.Models.Client> clients)
        {
            return clients.Select(c => c.ToDto()).ToList();
        }

        public static List<ScopeResponse> ToDtos(this ICollection<Scope> scopes)
        {
            return scopes.Select(s => s.ToDto()).ToList();
        }

        public static List<ResourceOwnerResponse> ToDtos(this ICollection<ResourceOwner> resourceOwners)
        {
            return resourceOwners.Select(r => r.ToDto()).ToList();
        }

        #endregion
    }
}
