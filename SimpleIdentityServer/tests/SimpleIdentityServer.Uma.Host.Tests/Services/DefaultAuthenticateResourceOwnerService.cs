using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Services;
using System.Threading.Tasks;

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
