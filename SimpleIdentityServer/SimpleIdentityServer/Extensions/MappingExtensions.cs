using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.DTOs.Response;
using SimpleIdentityServer.Api.ViewModels;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using Display = SimpleIdentityServer.Api.DTOs.Request.Display;
using ResponseMode = SimpleIdentityServer.Api.DTOs.Request.ResponseMode;

namespace SimpleIdentityServer.Api.Extensions
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

        public static LocalAuthenticationParameter ToParameter(this AuthorizeViewModel viewModel)
        {
            return new LocalAuthenticationParameter
            {
                UserName = viewModel.UserName,
                Password = viewModel.Password
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
                    Core.Models.ResponseType responseTypeEnum;
                    if (Enum.TryParse(responseType, out responseTypeEnum))
                    {
                        responseTypes.Add(responseTypeEnum);
                    }
                }
            }

            if(clientResponse.GrantTypes != null &&
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