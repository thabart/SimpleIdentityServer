using SimpleIdentityServer.Core.Errors;

namespace SimpleIdentityServer.Core.Parameters
{
    public sealed class GetAuthorizationCodeParameter : BaseRequestParameter
    {
        public string RedirectUrl { get; set; }

        public string State { get; set; }

        public string ResponseMode { get; set; }

        public string Nonce { get; set; }

        public DisplayParameter Display { get; set; }

        public PromptParameter Prompt { get; set; }

        public string MaxAge { get; set; }

        public string UiLocales { get; set; }

        public string IdTokenHint { get; set; }
        
        public string LoginHint { get; set; }

        public string AcrValues { get; set; }

        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(Scope))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "scope"));
            }

            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "clientId"));
            }
            
            if (string.IsNullOrWhiteSpace(RedirectUrl))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "redirect_uri"));
            }
        }
    }
}
