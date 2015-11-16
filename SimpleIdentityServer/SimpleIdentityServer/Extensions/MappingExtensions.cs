using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.ViewModels;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Api.Extensions
{
    public static class MappingExtensions
    {
        public static AuthorizationParameter ToParameter(this AuthorizationRequest request)
        {
            return new AuthorizationParameter
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
    }
}