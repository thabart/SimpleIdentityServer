using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Services
{
    public class InMemoryUserService : IResourceOwnerService
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        private readonly ISecurityHelper _securityHelper;

        private List<ResourceOwner> _resourceOwners;

        public InMemoryUserService(
            IResourceOwnerRepository resourceOwnerRepository,
            ISecurityHelper securityHelper)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _resourceOwners = new List<ResourceOwner>();
            _securityHelper = securityHelper;
        }

        public string Authenticate(string userName, string password)
        {
            var hashedPassword = _securityHelper.ComputeHash(password);
            var user = _resourceOwnerRepository.GetResourceOwnerByCredentials(userName, hashedPassword);
            if (user == null)
            {
                return null;
            }

            _resourceOwners.Add(user);
            return user.Id;
        }
    }
}
