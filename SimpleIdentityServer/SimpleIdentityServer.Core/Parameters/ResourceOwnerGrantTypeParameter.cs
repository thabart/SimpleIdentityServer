using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;

namespace SimpleIdentityServer.Core.Parameters
{
    public sealed class ResourceOwnerGrantTypeParameter
    {
        public string ClientId { get; set; }

        public string Scope { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public void Validate()
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

            if (string.IsNullOrWhiteSpace(UserName))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "username"));
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "password"));
            }
        }
    }
}
