using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Api.Mappings
{
    public static class MappingExtensions
    {
        public static GetAuthorizationParameter ToParameter(this AuthorizationRequest request)
        {
            return new GetAuthorizationParameter
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
    }
}