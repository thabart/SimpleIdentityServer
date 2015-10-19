using SimpleIdentityServer.Core.Errors;

namespace SimpleIdentityServer.Core.Parameters
{
    public sealed class GetAuthorizationCodeParameter : BaseRequestParameter
    {
        public string RedirectUrl { get; set; }

        public string State { get; set; }

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
        }
    }
}
