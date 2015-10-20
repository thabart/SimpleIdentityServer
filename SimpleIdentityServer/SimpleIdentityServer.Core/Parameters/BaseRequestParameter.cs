using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;

namespace SimpleIdentityServer.Core.Parameters
{
    public class BaseRequestParameter
    {
        public string ClientId { get; set; }

        public string Scope { get; set; }

        public virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "clientId"));
            }
        }
    }
}
