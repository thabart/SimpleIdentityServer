using System.Net.Http.Headers;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.Api.Token
{
    public interface ITokenActions
    {
        GrantedToken GetTokenByResourceOwnerCredentialsGrantType(
            ResourceOwnerGrantTypeParameter parameter);

        GrantedToken GetTokenByAuthorizationCodeGrantType(
            AuthorizationCodeGrantTypeParameter parameter,
            AuthenticationHeaderValue authenticationHeaderValue);
    }

    public class TokenActions : ITokenActions
    {
        private readonly IGetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantType;

        private readonly IResourceOwnerGrantTypeParameterValidator _resourceOwnerGrantTypeParameterValidator;

        private readonly IGetTokenByAuthorizationCodeGrantTypeAction _getTokenByAuthorizationCodeGrantTypeAction;

        private readonly IAuthorizationCodeGrantTypeParameterTokenEdpValidator _authorizationCodeGrantTypeParameterTokenEdpValidator;

        public TokenActions(
            IGetTokenByResourceOwnerCredentialsGrantTypeAction getTokenByResourceOwnerCredentialsGrantType,
            IGetTokenByAuthorizationCodeGrantTypeAction getTokenByAuthorizationCodeGrantTypeAction,
            IResourceOwnerGrantTypeParameterValidator resourceOwnerGrantTypeParameterValidator,
            IAuthorizationCodeGrantTypeParameterTokenEdpValidator authorizationCodeGrantTypeParameterTokenEdpValidator)
        {
            _getTokenByResourceOwnerCredentialsGrantType = getTokenByResourceOwnerCredentialsGrantType;
            _getTokenByAuthorizationCodeGrantTypeAction = getTokenByAuthorizationCodeGrantTypeAction;
            _resourceOwnerGrantTypeParameterValidator = resourceOwnerGrantTypeParameterValidator;
            _authorizationCodeGrantTypeParameterTokenEdpValidator = authorizationCodeGrantTypeParameterTokenEdpValidator;
        }

        public GrantedToken GetTokenByResourceOwnerCredentialsGrantType(
            ResourceOwnerGrantTypeParameter parameter)
        {
            _resourceOwnerGrantTypeParameterValidator.Validate(parameter);
            return _getTokenByResourceOwnerCredentialsGrantType.Execute(parameter);
        }

        public GrantedToken GetTokenByAuthorizationCodeGrantType(
            AuthorizationCodeGrantTypeParameter parameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            _authorizationCodeGrantTypeParameterTokenEdpValidator.Validate(parameter);
            return _getTokenByAuthorizationCodeGrantTypeAction.Execute(parameter, authenticationHeaderValue);
        }
    }
}
