using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.SMS.Actions
{
    public interface ISmsAuthenticationOperation
    {
        Task<ResourceOwner> Execute(string phoneNumber);
    }

    internal sealed class SmsAuthenticationOperation : ISmsAuthenticationOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public SmsAuthenticationOperation(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<ResourceOwner> Execute(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            var resourceOwner = await _resourceOwnerRepository.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber);
            if (resourceOwner == null)
            {
                throw new IdentityServerAuthenticationException("the phone number is not correct");
            }

            return resourceOwner;
        }
    }
}