using System;

namespace SimpleIdentityServer.Core.Parameters
{
    public enum ResponseMode
    {
        None,
        query,
        fragment
    }

    public enum Display
    {
        page,
        popup,
        touch,
        wap
    }

    [Flags]
    public enum PromptParameter
    {
        none,
        login,
        consent,
        select_account
    }

    [Flags]
    public enum ResponseTypeParameter
    {
        code,
        token,
        id_token
    }

    public sealed class AuthorizationParameter
    {
        public string ClientId { get; set; }

        public string Scope { get; set; }

        public string ResponseType { get; set; }

        public string RedirectUrl { get; set; }

        public string State { get; set; }

        public ResponseMode ResponseMode { get; set; }

        public string Nonce { get; set; }

        public Display Display { get; set; }

        public string Prompt { get; set; }

        public double MaxAge { get; set; }

        public string UiLocales { get; set; }

        public string IdTokenHint { get; set; }
        
        public string LoginHint { get; set; }

        public string AcrValues { get; set; }
    }
}
