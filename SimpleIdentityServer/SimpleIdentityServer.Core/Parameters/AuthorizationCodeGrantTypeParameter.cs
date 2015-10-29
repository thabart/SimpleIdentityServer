using System;
using System.Collections.Generic;

using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;

namespace SimpleIdentityServer.Core.Parameters
{
    public enum ResponseMode
    {
        None,
        query,
        fragment
    }

    public enum ResponseType
    {
        None,
        code,
        token
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

    public sealed class AuthorizationCodeGrantTypeParameter
    {
        public string ClientId { get; set; }

        public string Scope { get; set; }

        public ResponseType ResponseType { get; set; }

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

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Scope))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "scope"),
                    State);
            }

            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "clientId"),
                    State);
            }
            
            if (string.IsNullOrWhiteSpace(RedirectUrl))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "redirect_uri"),
                    State);
            }

            if (ResponseType == ResponseType.None)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "response_type"),
                    State);
            }
        }
    }
}
