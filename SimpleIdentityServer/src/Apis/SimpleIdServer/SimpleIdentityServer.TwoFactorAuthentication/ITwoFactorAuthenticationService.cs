using SimpleIdentityServer.Core.Common.Models;
using System.Threading.Tasks;

namespace SimpleIdentityServer.TwoFactorAuthentication
{
    public interface ITwoFactorAuthenticationService
    {
        Task SendAsync(string code, ResourceOwner user);
        string RequiredClaim { get; }
        string Name { get; }
    }
}
