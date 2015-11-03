using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IResourceOwnerGrantTypeParameterValidator
    {
        void Validate(ResourceOwnerGrantTypeParameter parameter);
    }

    public sealed class ResourceOwnerGrantTypeParameterValidator : IResourceOwnerGrantTypeParameterValidator
    {
        public void Validate(ResourceOwnerGrantTypeParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.ClientId))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "clientId"));
            }

            if (string.IsNullOrWhiteSpace(parameter.UserName))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "username"));
            }

            if (string.IsNullOrWhiteSpace(parameter.Password))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "password"));
            }
        }
    }
}
