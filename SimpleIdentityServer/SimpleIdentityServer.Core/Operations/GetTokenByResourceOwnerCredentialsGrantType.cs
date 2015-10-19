using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.DataAccess.Models;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Core.Operations
{
    public interface IGetTokenByResourceOwnerCredentialsGrantType
    {
        GrantedToken Execute(GetAccessTokenWithResourceOwnerCredentialsParameter parameter);
    }

    public class GetTokenByResourceOwnerCredentialsGrantType : IGetTokenByResourceOwnerCredentialsGrantType
    {
        private readonly IDataSource _dataSource;

        private readonly ITokenHelper _tokenHelper;

        private readonly IValidatorHelper _validatorHelper;

        public GetTokenByResourceOwnerCredentialsGrantType(
            IDataSource dataSource,
            ITokenHelper tokenHelper,
            IValidatorHelper validatorHelper)
        {
            _dataSource = dataSource;
            _tokenHelper = tokenHelper;
            _validatorHelper = validatorHelper;
        }

        public GrantedToken Execute(
            GetAccessTokenWithResourceOwnerCredentialsParameter parameter)
        {
            parameter.Validate();
            var client = _validatorHelper.ValidateExistingClient(parameter.ClientId);
            _validatorHelper.ValidateResourceOwner(parameter.UserName, parameter.Password);

            var allowedTokenScopes = string.Empty;

            if (!string.IsNullOrWhiteSpace(parameter.Scope))
            {
                allowedTokenScopes = _validatorHelper.ValidateAllowedScopes(parameter.Scope, client);
            }

            var generatedToken = _tokenHelper.GenerateToken(allowedTokenScopes);
            _dataSource.GrantedTokens.Add(generatedToken);
            _dataSource.SaveChanges();

            return generatedToken;
        }
    }
}