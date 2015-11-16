using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface IGrantedTokenRepository
    {
        bool Insert(GrantedToken grantedToken);

        GrantedToken GetToken(string accessToken);
    }
}
