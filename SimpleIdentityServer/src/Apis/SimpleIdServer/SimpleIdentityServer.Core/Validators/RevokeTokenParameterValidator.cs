using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using System;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IRevokeTokenParameterValidator
    {
        void Validate(RevokeTokenParameter parameter);
    }

    internal sealed class RevokeTokenParameterValidator : IRevokeTokenParameterValidator
    {
        public void Validate(RevokeTokenParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            // Read this RFC for more information
            if (string.IsNullOrWhiteSpace(parameter.Token))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.IntrospectionRequestNames.Token));
            }
        }
    }
}
