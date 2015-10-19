using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Core.Operations
{
    public interface IGetAuthorizationCodeOperation
    {

    }

    public class GetAuthorizationCodeOperation : IGetAuthorizationCodeOperation
    {
        private readonly IDataSource _dataSource;

        private readonly ITokenHelper _tokenHelper;

        private readonly IValidatorHelper _validatorHelper;

        public GetAuthorizationCodeOperation(
            IDataSource dataSource,
            ITokenHelper tokenHelper,
            IValidatorHelper validatorHelper)
        {
            _dataSource = dataSource;
            _tokenHelper = tokenHelper;
            _validatorHelper = validatorHelper;
        }

        public void Execute(GetAuthorizationCodeParameter parameter)
        {
            try
            {
                parameter.Validate();
                var client = _validatorHelper.ValidateExistingClient(parameter.ClientId);
                _validatorHelper.ValidateAllowedRedirectionUrl(parameter.RedirectUrl, client);
                var allowedScopes = _validatorHelper.ValidateAllowedScopes(parameter.Scope, client);

            }
            catch (IdentityServerException ex)
            {
                throw new IdentityServerExceptionWithState(ex, parameter.State);
            }
        }
    }
}
