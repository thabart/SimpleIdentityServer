using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Services;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    public interface IUpdateUserCredentialsOperation
    {
        Task<bool> Execute(string subject, string newPassword);
    }

    internal sealed class UpdateUserCredentialsOperation : IUpdateUserCredentialsOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

        public UpdateUserCredentialsOperation(IResourceOwnerRepository resourceOwnerRepository, IAuthenticateResourceOwnerService authenticateResourceOwnerService)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
        }

        public async Task<bool> Execute(string subject, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentNullException(nameof(newPassword));
            }

            var resourceOwner = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(subject);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheRoDoesntExist);
            }

            resourceOwner.Password = _authenticateResourceOwnerService.GetHashedPassword(newPassword);
            return await _resourceOwnerRepository.UpdateAsync(resourceOwner);
        }
    }
}
