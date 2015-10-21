using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public sealed class GetAuthorizationParameter
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

        public List<PromptParameter> GetPromptParameters()
        {
            var defaultResult = new List<PromptParameter>
            {
                PromptParameter.none
            };

            var promptValues = Enum.GetNames(typeof(PromptParameter));
            if (string.IsNullOrWhiteSpace(Prompt))
            {
                return defaultResult;
            }

            var prompts = Prompt.Split(' ')
                .Where(c => !string.IsNullOrWhiteSpace(c) && promptValues.Contains(c))
                .Select(c => (PromptParameter)Enum.Parse(typeof(PromptParameter), c))
                .ToList();
            if (prompts == null || !prompts.Any())
            {
                prompts = defaultResult;
            }

            return prompts;
        }
    }
}
