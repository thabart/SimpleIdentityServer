using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface IAuthorizationCodeRepository
    {
        bool AddAuthorizationCode(AuthorizationCode authorizationCode);

        AuthorizationCode GetAuthorizationCode(string code);

        bool RemoveAuthorizationCode(string code);
    }
}
