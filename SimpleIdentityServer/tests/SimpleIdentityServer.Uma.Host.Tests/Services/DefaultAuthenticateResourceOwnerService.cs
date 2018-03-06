using System.Threading.Tasks;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Services;

namespace SimpleIdentityServer.Uma.Host.Tests.Services
{
    public class DefaultAuthenticateResourceOwnerService : IAuthenticateResourceOwnerService
    {
        public Task<ResourceOwner> AuthenticateResourceOwnerAsync(string login)
        {
            throw new System.NotImplementedException();
        }

        public Task<ResourceOwner> AuthenticateResourceOwnerAsync(string login, string password)
        {
            throw new System.NotImplementedException();
        }

        public string GetHashedPassword(string password)
        {
            throw new System.NotImplementedException();
        }
    }
}
