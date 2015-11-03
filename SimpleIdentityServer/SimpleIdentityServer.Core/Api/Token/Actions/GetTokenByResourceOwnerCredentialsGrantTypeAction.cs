using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IGetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        GrantedToken Execute(ResourceOwnerGrantTypeParameter parameter);
    }

    public class GetTokenByResourceOwnerCredentialsGrantTypeAction : IGetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        private readonly IGrantedTokenRepository _grantedTokenRepository;

        private readonly ITokenHelper _tokenHelper;
        
        private readonly IClientValidator _clientValidator;

        private readonly IScopeValidator _scopeValidator;

        private readonly IResourceOwnerValidator _resourceOwnerValidator;

        public GetTokenByResourceOwnerCredentialsGrantTypeAction(
            IGrantedTokenRepository grantedTokenRepository,
            ITokenHelper tokenHelper,
            IClientValidator clientValidator,
            IScopeValidator scopeValidator,
            IResourceOwnerValidator resourceOwnerValidator)
        {
            _grantedTokenRepository = grantedTokenRepository;
            _tokenHelper = tokenHelper;
            _clientValidator = clientValidator;
            _scopeValidator = scopeValidator;
            _resourceOwnerValidator = resourceOwnerValidator;
        }

        public GrantedToken Execute(
            ResourceOwnerGrantTypeParameter parameter)
        {
            var client = _clientValidator.ValidateClientExist(parameter.ClientId);
            _resourceOwnerValidator.ValidateResourceOwnerCredentials(parameter.UserName, parameter.Password);

            var allowedTokenScopes = string.Empty;

            if (!string.IsNullOrWhiteSpace(parameter.Scope))
            {
                allowedTokenScopes = string.Join(" ", _scopeValidator.ValidateAllowedScopes(parameter.Scope, client));
            }

            var generatedToken = _tokenHelper.GenerateToken(allowedTokenScopes);
            _grantedTokenRepository.Insert(generatedToken);

            return generatedToken;
        }
    }
}