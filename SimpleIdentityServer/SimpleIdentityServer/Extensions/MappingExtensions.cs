﻿using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.ViewModels;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Api.Extensions
{
    public static class MappingExtensions
    {
        public static AuthorizationCodeGrantTypeParameter ToParameter(this AuthorizationRequest request)
        {
            return new AuthorizationCodeGrantTypeParameter
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
                ResponseType = (Core.Parameters.ResponseType)request.response_type,
                Scope = request.scope,
                State = request.state,
                UiLocales = request.ui_locales
            };
        }

        public static LocalAuthorizationParameter ToParameter(this AuthorizeViewModel viewModel)
        {
            return new LocalAuthorizationParameter
            {
                UserName = viewModel.UserName,
                Password = viewModel.Password
            };
        }

        public static ResourceOwnerGrantTypeParameter ToParameter(this TokenRequest request)
        {
            return new ResourceOwnerGrantTypeParameter
            {
                ClientId = request.client_id,
                UserName = request.username,
                Password = request.password,
                Scope = request.scope
            };
        }
    }
}