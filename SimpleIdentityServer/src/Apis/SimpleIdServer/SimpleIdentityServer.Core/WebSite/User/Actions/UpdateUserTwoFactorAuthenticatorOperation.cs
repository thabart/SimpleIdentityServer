using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Services;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    public interface IUpdateUserTwoFactorAuthenticatorOperation
    {
        Task<bool> Execute(string subject, string twoFactorAuth);
    }

    internal sealed class UpdateUserTwoFactorAuthenticatorOperation : IUpdateUserTwoFactorAuthenticatorOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

        public UpdateUserTwoFactorAuthenticatorOperation(IResourceOwnerRepository resourceOwnerRepository, IAuthenticateResourceOwnerService authenticateResourceOwnerService)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
        }

        public async Task<bool> Execute(string subject, string twoFactorAuth)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var resourceOwner = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(subject);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheRoDoesntExist);
            }

            resourceOwner.TwoFactorAuthentication = twoFactorAuth;
            return await _resourceOwnerRepository.UpdateAsync(resourceOwner);
        }
    }
}
