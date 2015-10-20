using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;

namespace SimpleIdentityServer.Core.Parameters
{
    public sealed class GetAccessTokenWithResourceOwnerCredentialsParameter : BaseRequestParameter
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public override void Validate()
        {
            base.Validate();
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
