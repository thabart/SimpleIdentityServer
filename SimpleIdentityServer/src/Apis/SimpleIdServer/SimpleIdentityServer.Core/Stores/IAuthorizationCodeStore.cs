using SimpleIdentityServer.Core.Common.Models;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Stores
{
    public interface IAuthorizationCodeStore
    {
        Task<AuthorizationCode> GetAuthorizationCode(string code);
        Task<bool> AddAuthorizationCode(AuthorizationCode authorizationCode);
        Task<bool> RemoveAuthorizationCode(string code);
    }
}
