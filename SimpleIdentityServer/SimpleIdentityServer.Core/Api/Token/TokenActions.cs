using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Core.Api.Token
{
    public interface ITokenActions
    {
        GrantedToken GetTokenByResourceOwnerCredentialsGrantType(
            ResourceOwnerGrantTypeParameter parameter);
    }

    public class TokenActions : ITokenActions
    {
        private readonly IGetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantType;

        public TokenActions(IGetTokenByResourceOwnerCredentialsGrantTypeAction getTokenByResourceOwnerCredentialsGrantType)
        {
            _getTokenByResourceOwnerCredentialsGrantType = getTokenByResourceOwnerCredentialsGrantType;
        }

        public GrantedToken GetTokenByResourceOwnerCredentialsGrantType(
            ResourceOwnerGrantTypeParameter parameter)
        {
            return _getTokenByResourceOwnerCredentialsGrantType.Execute(parameter);
        }
    }
}
