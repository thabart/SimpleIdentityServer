using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.ViewModels;
using SimpleIdentityServer.Core.Jwt;
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

        public static LocalAuthorizationParameter ToParameter(this AuthorizeViewModel viewModel)
        {
            return new LocalAuthorizationParameter
            {
                UserName = viewModel.UserName,
                Password = viewModel.Password
            };
        }

        public static ResourceOwnerGrantTypeParameter ToResourceOwnerGrantTypeParameter(this TokenRequest request)
        {
            return new ResourceOwnerGrantTypeParameter
            {
                ClientId = request.client_id,
                UserName = request.username,
                Password = request.password,
                Scope = request.scope
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
                responseModeEnum = ResponseMode.query;
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