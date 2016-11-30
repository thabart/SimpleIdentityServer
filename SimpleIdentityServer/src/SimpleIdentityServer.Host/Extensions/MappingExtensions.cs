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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.Host.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using Display = SimpleIdentityServer.Host.DTOs.Request.Display;
using ResponseMode = SimpleIdentityServer.Host.DTOs.Request.ResponseMode;

namespace SimpleIdentityServer.Host.Extensions
{
    public static class MappingExtensions
    {
        public static AuthorizationParameter ToParameter(this AuthorizationRequest request)
        {
            var result = new AuthorizationParameter
            {
                AcrValues = request.acr_values,
                ClientId = request.client_id,
                Display = (Core.Parameters.Display)request.display,
                IdTokenHint = request.id_token_hint,
                LoginHint = request.login_hint,
                MaxAge = request.max_age,
                Nonce = request.nonce,
                Prompt = request.prompt,
                RedirectUrl = request.redirect_uri,
                ResponseMode  = (Core.Parameters.ResponseMode)request.response_mode,
                ResponseType = request.response_type,
                Scope = request.scope,
                State = request.state,
                UiLocales = request.ui_locales
            };

            if (!string.IsNullOrWhiteSpace(request.claims))
            {
                var claimsParameter = new ClaimsParameter();
                result.Claims = claimsParameter;

                var obj = JObject.Parse(request.claims);
                var idToken = obj.GetValue(Core.Constants.StandardClaimParameterNames.IdTokenName);
                var userInfo = obj.GetValue(Core.Constants.StandardClaimParameterNames.UserInfoName);
                if (idToken != null)
                {
                    claimsParameter.IdToken = new List<ClaimParameter>();
                    FillInClaimsParameter(idToken, claimsParameter.IdToken);
                }

                if (userInfo != null)
                {
                    claimsParameter.UserInfo = new List<ClaimParameter>();
                    FillInClaimsParameter(userInfo, claimsParameter.UserInfo);
                }

                result.Claims = claimsParameter;
            }

            return result;
        }

        public static RevokeTokenParameter ToParameter(this RevocationRequest revocationRequest)
        {
            return new RevokeTokenParameter
            {
                ClientAssertion = revocationRequest.ClientAssertion,
                ClientAssertionType = revocationRequest.ClientAssertionType,
                ClientId = revocationRequest.ClientId,
                ClientSecret = revocationRequest.ClientSecret,
                Token = revocationRequest.Token,
                TokenTypeHint = revocationRequest.TokenTypeHint
            };
        }

        public static IntrospectionParameter ToParameter(this IntrospectionRequest viewModel)
        {
            return new IntrospectionParameter
            {
                ClientAssertion = viewModel.client_assertion,
                ClientAssertionType = viewModel.client_assertion_type,
                ClientId = viewModel.client_id,
                ClientSecret = viewModel.client_secret,
                Token = viewModel.token,
                TokenTypeHint = viewModel.token_type_hint
            };
        }

        public static ResourceOwnerGrantTypeParameter ToResourceOwnerGrantTypeParameter(this TokenRequest request)
        {
            return new ResourceOwnerGrantTypeParameter
            {
                UserName = request.username,
                Password = request.password,
                Scope = request.scope,
                ClientId = request.client_id,
                ClientAssertion = request.client_assertion,
                ClientAssertionType = request.client_assertion_type,
                ClientSecret = request.client_secret
            };
        }

        public static AuthorizationCodeGrantTypeParameter ToAuthorizationCodeGrantTypeParameter(this TokenRequest request)
        {
            return new AuthorizationCodeGrantTypeParameter
            {
                ClientId = request.client_id,
                ClientSecret = request.client_secret,
                Code = request.code,
                RedirectUri = request.redirect_uri,
                ClientAssertion = request.client_assertion,
                ClientAssertionType = request.client_assertion_type
            };
        }

        public static RefreshTokenGrantTypeParameter ToRefreshTokenGrantTypeParameter(this TokenRequest request)
        {
            return new RefreshTokenGrantTypeParameter
            {
                ClientId = request.client_id,
                ClientSecret = request.client_secret,
                RefreshToken = request.refresh_token,
                RedirectUri = request.redirect_uri,
                ClientAssertion = request.client_assertion,
                ClientAssertionType = request.client_assertion_type
            };
        }

        public static ClientCredentialsGrantTypeParameter ToClientCredentialsGrantTypeParameter(this TokenRequest request)
        {
            return new ClientCredentialsGrantTypeParameter
            {
                ClientAssertion = request.client_assertion,
                ClientAssertionType = request.client_assertion_type,
                ClientId = request.client_id,
                ClientSecret = request.client_secret,
                Scope = request.scope
            };
        }

        public static AuthorizationRequest ToAuthorizationRequest(this JwsPayload jwsPayload)
        {
            Display displayEnum;
            ResponseMode responseModeEnum;
            var displayVal =
                jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.DisplayName);
            var responseMode =
                jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.ResponseModeName);
            if (string.IsNullOrWhiteSpace(displayVal) || !Enum.TryParse(displayVal, out displayEnum))
            {
                displayEnum = Display.page;
            }

            if (string.IsNullOrWhiteSpace(responseMode) || !Enum.TryParse(responseMode, out responseModeEnum))
            {
                responseModeEnum = ResponseMode.None;
            }

            var result = new AuthorizationRequest
            {
                acr_values = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.AcrValuesName),
                claims = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.ClaimsName),
                client_id = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.ClientIdName),
                display = displayEnum,
                prompt = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.PromptName),
                id_token_hint = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.IdTokenHintName),
                max_age = jwsPayload.GetDoubleClaim(Core.Constants.StandardAuthorizationRequestParameterNames.MaxAgeName),
                nonce = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.NonceName),
                response_type = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.ResponseTypeName),
                state = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.StateName),
                login_hint = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.LoginHintName),
                redirect_uri = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.RedirectUriName),
                request = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.RequestName),
                request_uri = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.RequestUriName),
                scope = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.ScopeName),
                response_mode = responseModeEnum,
                ui_locales = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.UiLocalesName),
            };

            return result;
        }

        public static JwsPayload ToJwsPayload(this AuthorizationRequest request)
        {
            return new JwsPayload
            {
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.AcrValuesName, request.acr_values
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ClaimsName, request.claims
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ClientIdName, request.client_id
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.DisplayName, Enum.GetName(typeof(Display), request.display)
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.PromptName, request.prompt
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.IdTokenHintName, request.id_token_hint
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.MaxAgeName, request.max_age
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.NonceName, request.nonce
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ResponseTypeName, request.response_type
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.StateName, request.state
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.LoginHintName, request.login_hint
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.RedirectUriName, request.redirect_uri
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.RequestName, request.request
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.RequestUriName, request.request_uri
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ScopeName, request.scope
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ResponseModeName, Enum.GetName(typeof(ResponseMode), request.response_mode)
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.UiLocalesName, request.ui_locales
                }
            };
        }

        public static RegistrationParameter ToParameter(this ClientResponse clientResponse)
        {
            var responseTypes = new List<Core.Models.ResponseType>();
            var redirectUris = clientResponse.redirect_uris == null
                ? new List<string>()
                : clientResponse.redirect_uris.ToList();
            var grantTypes = new List<GrantType>();
            ApplicationTypes? applicationType = null;
            if (clientResponse.response_types != null &&
                clientResponse.response_types.Any())
            {
                foreach (var responseType in clientResponse.response_types)
                {
                    var responseTypeSplitted = responseType.Split(' ');
                    foreach (var response in responseTypeSplitted)
                    {
                        Core.Models.ResponseType responseTypeEnum;
                        if (Enum.TryParse(response, out responseTypeEnum) &&
                            !responseTypes.Contains(responseTypeEnum))
                        {
                            responseTypes.Add(responseTypeEnum);
                        }
                    }
                }
            }

            if(clientResponse.grant_types != null &&
                clientResponse.grant_types.Any())
            {
                foreach (var grantType in clientResponse.grant_types)
                {
                    GrantType grantTypeEnum;
                    if (Enum.TryParse(grantType, out grantTypeEnum))
                    {
                        grantTypes.Add(grantTypeEnum);
                    }
                }
            }

            ApplicationTypes appTypeEnum;
            if (Enum.TryParse(clientResponse.application_type, out appTypeEnum))
            {
                applicationType = appTypeEnum;
            }

            return new RegistrationParameter
            {
                ApplicationType = applicationType,
                ClientName = clientResponse.client_name,
                ClientUri = clientResponse.client_uri,
                Contacts = clientResponse.contacts == null ? new List<string>() : clientResponse.contacts.ToList(),
                DefaultAcrValues = clientResponse.default_acr_values,
                DefaultMaxAge = clientResponse.default_max_age,
                GrantTypes = grantTypes,
                IdTokenEncryptedResponseAlg = clientResponse.id_token_encrypted_response_alg,
                IdTokenEncryptedResponseEnc = clientResponse.id_token_encrypted_response_enc,
                IdTokenSignedResponseAlg = clientResponse.id_token_signed_response_alg,
                InitiateLoginUri = clientResponse.initiate_login_uri,
                Jwks = clientResponse.jwks,
                JwksUri = clientResponse.jwks_uri,
                LogoUri = clientResponse.logo_uri,
                PolicyUri = clientResponse.policy_uri,
                RedirectUris = redirectUris,
                RequestObjectEncryptionAlg = clientResponse.request_object_encryption_alg,
                RequestObjectEncryptionEnc = clientResponse.request_object_encryption_enc,
                RequestObjectSigningAlg = clientResponse.request_object_signing_alg,
                RequestUris = clientResponse.request_uris,
                RequireAuthTime = clientResponse.require_auth_time,
                ResponseTypes = responseTypes,
                SectorIdentifierUri = clientResponse.sector_identifier_uri,
                SubjectType = clientResponse.subject_type,
                TokenEndPointAuthMethod = clientResponse.token_endpoint_auth_method,
                TokenEndPointAuthSigningAlg = clientResponse.token_endpoint_auth_signing_alg,
                TosUri = clientResponse.tos_uri,
                UserInfoEncryptedResponseAlg = clientResponse.userinfo_encrypted_response_alg,
                UserInfoEncryptedResponseEnc = clientResponse.userinfo_encrypted_response_enc,
                UserInfoSignedResponseAlg = clientResponse.userinfo_signed_response_alg,
                ScimProfile = clientResponse.scim_profile
            };
        }

        public static IntrospectionResponse ToDto(this IntrospectionResult introspectionResult)
        {
            return new IntrospectionResponse
            {
                Active = introspectionResult.Active,
                Audience = introspectionResult.Audience,
                ClientId = introspectionResult.ClientId,
                Expiration = introspectionResult.Expiration,
                IssuedAt = introspectionResult.IssuedAt,
                Issuer = introspectionResult.Issuer,
                Jti = introspectionResult.Jti,
                Nbf = introspectionResult.Nbf,
                Scope = introspectionResult.Scope.Split(' ').ToList(),
                Subject = introspectionResult.Subject,
                TokenType = introspectionResult.TokenType,
                UserName = introspectionResult.UserName
            };
        }

        public static TokenResponse ToDto(this GrantedToken grantedToken)
        {
            if (grantedToken == null)
            {
                throw new ArgumentNullException(nameof(grantedToken));
            }

            return new TokenResponse
            {
                AccessToken = grantedToken.AccessToken,
                IdToken = grantedToken.IdToken,
                ExpiresIn = grantedToken.ExpiresIn,
                RefreshToken = grantedToken.RefreshToken,
                TokenType = grantedToken.TokenType,
                Scope = grantedToken.Scope.Split(' ').ToList()
            };
        }

        private static void FillInClaimsParameter(
            JToken token,
            List<ClaimParameter> claimParameters)
        {
            foreach (var child in token.Children())
            {
                var record = new ClaimParameter
                {
                    Name = ((JProperty)child).Name,
                    Parameters = new Dictionary<string, object>()
                };
                claimParameters.Add(record);

                var subChild = child.Children().FirstOrDefault();
                if (subChild == null)
                {
                    continue;
                }

                var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(subChild.ToString());
                record.Parameters = parameters;
            }
        }
    }
}