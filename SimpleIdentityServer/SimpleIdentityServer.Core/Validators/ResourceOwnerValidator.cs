using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IResourceOwnerValidator
    {
        ResourceOwner ValidateResourceOwnerCredentials(string userName, string password);
    }

    public class ResourceOwnerValidator : IResourceOwnerValidator
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        private readonly ISecurityHelper _securityHelper;

        public ResourceOwnerValidator(
            IResourceOwnerRepository resourceOwnerRepository,
            ISecurityHelper securityHelper)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _securityHelper = securityHelper;
        }

        public ResourceOwner ValidateResourceOwnerCredentials(string userName, string password)
        {
            var hashPassword = _securityHelper.ComputeHash(password);
            var resourceOwner = _resourceOwnerRepository.GetResourceOwnerByCredentials(userName, hashPassword);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidGrant,
                    ErrorDescriptions.ResourceOwnerCredentialsAreNotValid);
            }

            return resourceOwner;
        }
    }
}
