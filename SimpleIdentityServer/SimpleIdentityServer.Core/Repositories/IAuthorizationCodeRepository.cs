using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface IAuthorizationCodeRepository
    {
        bool AddAuthorizationCode(AuthorizationCode authorizationCode);
    }
}
