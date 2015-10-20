using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface IResourceOwnerRepository
    {
        ResourceOwner GetResourceOwnerByCredentials(string userName, string hashedPassword);

        bool Insert(ResourceOwner resourceOwner);
    }
}
