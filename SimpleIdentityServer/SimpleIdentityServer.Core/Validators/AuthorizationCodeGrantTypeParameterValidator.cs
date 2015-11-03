using System;
using System.Linq;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IAuthorizationCodeGrantTypeParameterValidator
    {
        void Validate(AuthorizationCodeGrantTypeParameter parameter);
    }

    public sealed class AuthorizationCodeGrantTypeParameterValidator : IAuthorizationCodeGrantTypeParameterValidator
    {
        private readonly IParameterParserHelper _parameterParserHelper;

        public AuthorizationCodeGrantTypeParameterValidator(IParameterParserHelper parameterParserHelper)
        {
            _parameterParserHelper = parameterParserHelper;
        }

        public void Validate(AuthorizationCodeGrantTypeParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.Scope))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "scope"),
                    parameter.State);
            }

            if (string.IsNullOrWhiteSpace(parameter.ClientId))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "clientId"),
                    parameter.State);
            }

            if (string.IsNullOrWhiteSpace(parameter.RedirectUrl))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "redirect_uri"),
                    parameter.State);
            }

            if (string.IsNullOrWhiteSpace(parameter.ResponseType))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "response_type"),
                    parameter.State);
            }

            ValidateResponseTypeParameter(parameter.ResponseType, parameter.State);
            ValidatePromptParameter(parameter.Prompt, parameter.State);

            // With this instruction
            // The redirect_uri is considered well-formed according to the RFC-3986
            var redirectUrlIsCorrect = Uri.IsWellFormedUriString(parameter.RedirectUrl, UriKind.Absolute);
            if (!redirectUrlIsCorrect)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestUriCode,
                    ErrorDescriptions.TheRedirectionUriIsNotWellFormed,
                    parameter.State);
            }
        }

        /// <summary>
        /// Validate the response type parameter.
        /// Returns an exception if at least one response_type parameter is not supported
        /// </summary>
        /// <param name="responseType"></param>
        /// <param name="state"></param>
        private void ValidateResponseTypeParameter(
            string responseType, 
            string state)
        {
            if (string.IsNullOrWhiteSpace(responseType))
            {
                return;
            }

            var responseTypeNames = Enum.GetNames(typeof(ResponseType));
            var atLeastOneResonseTypeIsNotSupported = responseType.Split(' ')
                .Any(r => !string.IsNullOrWhiteSpace(r) && !responseTypeNames.Contains(r));
            if (atLeastOneResonseTypeIsNotSupported)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.AtLeastOneResponseTypeIsNotSupported,
                    state);
            }
        }

        /// <summary>
        /// Validate the prompt parameter.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="state"></param>
        private void ValidatePromptParameter(
            string prompt,
            string state)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return;
            }

            var promptNames = Enum.GetNames(typeof (PromptParameter));
            var atLeastOnePromptIsNotSupported = prompt.Split(' ')
                .Any(r => !string.IsNullOrWhiteSpace(r) && !promptNames.Contains(r));
            if (atLeastOnePromptIsNotSupported)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.AtLeastOnePromptIsNotSupported,
                    state);
            }

            var prompts = _parameterParserHelper.ParsePromptParameters(prompt);
            if (prompts.Contains(PromptParameter.none) &&
                (prompts.Contains(PromptParameter.login) ||
                prompts.Contains(PromptParameter.consent) ||
                prompts.Contains(PromptParameter.select_account)))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.PromptParameterShouldHaveOnlyNoneValue,
                    state
                    );
            }
        }
    }
}
