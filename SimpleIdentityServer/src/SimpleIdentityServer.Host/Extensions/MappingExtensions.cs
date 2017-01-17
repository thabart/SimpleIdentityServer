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
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Host.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Host.Extensions
{
    public static class MappingExtensions
    {
        public static AuthorizationParameter ToParameter(this AuthorizationRequest request)
        {
            var result = new AuthorizationParameter
            {
                AcrValues = request.AcrValues,
                ClientId = request.ClientId,
                Display = request.Display == null ? Core.Parameters.Display.page : (Core.Parameters.Display)request.Display,
                IdTokenHint = request.IdTokenHint,
                LoginHint = request.LoginHint,
                MaxAge = request.MaxAge,
                Nonce = request.Nonce,
                Prompt = request.Prompt,
                RedirectUrl = request.RedirectUri,
                ResponseMode  = request.ResponseMode == null ? Core.Parameters.ResponseMode.None : (Core.Parameters.ResponseMode)request.ResponseMode,
                ResponseType = request.ResponseType,
                Scope = request.Scope,
                State = request.State,
                UiLocales = request.UiLocales
            };

            if (!string.IsNullOrWhiteSpace(request.Claims))
            {
                var claimsParameter = new ClaimsParameter();
                result.Claims = claimsParameter;

                var obj = JObject.Parse(request.Claims);
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

            if (!string.IsNullOrWhiteSpace(request.CodeChallenge) && request.CodeChallengeMethod != null)
            {
                result.CodeChallenge = request.CodeChallenge;
                result.CodeChallengeMethod = (Core.Parameters.CodeChallengeMethods)request.CodeChallengeMethod;
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
                ClientAssertion = viewModel.ClientAssertion,
                ClientAssertionType = viewModel.ClientAssertionType,
                ClientId = viewModel.ClientId,
                ClientSecret = viewModel.ClientSecret,
                Token = viewModel.Token,
                TokenTypeHint = viewModel.TokenTypeHint
            };
        }

        public static ResourceOwnerGrantTypeParameter ToResourceOwnerGrantTypeParameter(this TokenRequest request)
        {
            return new ResourceOwnerGrantTypeParameter
            {
                UserName = request.Username,
                Password = request.Password,
                Scope = request.Scope,
                ClientId = request.ClientId,
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                ClientSecret = request.ClientSecret
            };
        }

        public static AuthorizationCodeGrantTypeParameter ToAuthorizationCodeGrantTypeParameter(this TokenRequest request)
        {
            return new AuthorizationCodeGrantTypeParameter
            {
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                Code = request.Code,
                RedirectUri = request.RedirectUri,
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                CodeVerifier = request.CodeVerifier
            };
        }

        public static RefreshTokenGrantTypeParameter ToRefreshTokenGrantTypeParameter(this TokenRequest request)
        {
            return new RefreshTokenGrantTypeParameter
            {
                RefreshToken = request.RefreshToken
            };
        }

        public static ClientCredentialsGrantTypeParameter ToClientCredentialsGrantTypeParameter(this TokenRequest request)
        {
            return new ClientCredentialsGrantTypeParameter
            {
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                Scope = request.Scope
            };
        }

        public static AuthorizationRequest ToAuthorizationRequest(this JwsPayload jwsPayload)
        {
            DisplayModes displayEnum;
            ResponseModes responseModeEnum;
            var displayVal =
                jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.DisplayName);
            var responseMode =
                jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.ResponseModeName);
            if (string.IsNullOrWhiteSpace(displayVal) || !Enum.TryParse(displayVal, out displayEnum))
            {
                displayEnum = DisplayModes.Page;
            }

            if (string.IsNullOrWhiteSpace(responseMode) || !Enum.TryParse(responseMode, out responseModeEnum))
            {
                responseModeEnum = ResponseModes.None;
            }

            var result = new AuthorizationRequest
            {
                AcrValues = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.AcrValuesName),
                Claims = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.ClaimsName),
                ClientId = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.ClientIdName),
                Display = displayEnum,
                Prompt = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.PromptName),
                IdTokenHint= jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.IdTokenHintName),
                MaxAge = jwsPayload.GetDoubleClaim(Core.Constants.StandardAuthorizationRequestParameterNames.MaxAgeName),
                Nonce = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.NonceName),
                ResponseType = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.ResponseTypeName),
                State = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.StateName),
                LoginHint = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.LoginHintName),
                RedirectUri = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.RedirectUriName),
                Request = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.RequestName),
                RequestUri = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.RequestUriName),
                Scope = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.ScopeName),
                ResponseMode = responseModeEnum,
                UiLocales = jwsPayload.GetClaimValue(Core.Constants.StandardAuthorizationRequestParameterNames.UiLocalesName),
            };

            return result;
        }

        public static JwsPayload ToJwsPayload(this AuthorizationRequest request)
        {
            return new JwsPayload
            {
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.AcrValuesName, request.AcrValues
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ClaimsName, request.Claims
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ClientIdName, request.ClientId
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.DisplayName, Enum.GetName(typeof(Display), request.Display)
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.PromptName, request.Prompt
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.IdTokenHintName, request.IdTokenHint
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.MaxAgeName, request.MaxAge
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.NonceName, request.Nonce
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ResponseTypeName, request.ResponseType
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.StateName, request.State
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.LoginHintName, request.LoginHint
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.RedirectUriName, request.RedirectUri
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.RequestName, request.Request
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.RequestUriName, request.RequestUri
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ScopeName, request.Scope
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.ResponseModeName, Enum.GetName(typeof(ResponseModes), request.ResponseMode)
                },
                {
                    Core.Constants.StandardAuthorizationRequestParameterNames.UiLocalesName, request.UiLocales
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

        public static Introspection ToDto(this IntrospectionResult introspectionResult)
        {
            return new Introspection
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