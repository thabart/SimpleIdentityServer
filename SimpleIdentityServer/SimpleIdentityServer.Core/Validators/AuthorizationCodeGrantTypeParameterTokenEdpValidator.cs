using System;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IAuthorizationCodeGrantTypeParameterTokenEdpValidator
    {
        void Validate(AuthorizationCodeGrantTypeParameter parameter);
    }

    public class AuthorizationCodeGrantTypeParameterTokenEdpValidator : IAuthorizationCodeGrantTypeParameterTokenEdpValidator
    {
        public void Validate(AuthorizationCodeGrantTypeParameter parameter)
        {
            /*
            if (string.IsNullOrWhiteSpace(parameter.ClientId))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "client_id"));
            }
            */

            if (string.IsNullOrWhiteSpace(parameter.Code))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "code"));
            }

            // With this instruction
            // The redirect_uri is considered well-formed according to the RFC-3986
            var redirectUrlIsCorrect = Uri.IsWellFormedUriString(parameter.RedirectUri, UriKind.Absolute);
            if (!redirectUrlIsCorrect)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestUriCode,
                    ErrorDescriptions.TheRedirectionUriIsNotWellFormed);
            }
        }
    }
}
