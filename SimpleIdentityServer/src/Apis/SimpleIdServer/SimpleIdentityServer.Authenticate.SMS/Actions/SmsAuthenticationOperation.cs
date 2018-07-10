using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.WebSite.User;
using System;
using System.Collections.Generic;
using System.Security.Claims;
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
        private readonly IUserActions _userActions; 
        private readonly SmsAuthenticationOptions _smsAuthenticationOptions;

        public SmsAuthenticationOperation(IResourceOwnerRepository resourceOwnerRepository, IUserActions userActions, SmsAuthenticationOptions smsAuthenticationOptions)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _userActions = userActions;
            _smsAuthenticationOptions = smsAuthenticationOptions;
        }

        public async Task<ResourceOwner> Execute(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }
            
            var resourceOwner = await _resourceOwnerRepository.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber);
            if (resourceOwner != null)
            {
                return resourceOwner;
            }

            var id = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber)
            };
            var record = new AddUserParameter(id, Guid.NewGuid().ToString(), claims);
            if (_smsAuthenticationOptions.IsScimResourceAutomaticallyCreated)
            {
                await _userActions.AddUser(record, new AuthenticationParameter
                {
                    ClientId = _smsAuthenticationOptions.AuthenticationOptions.ClientId,
                    ClientSecret = _smsAuthenticationOptions.AuthenticationOptions.ClientSecret,
                    WellKnownAuthorizationUrl = _smsAuthenticationOptions.AuthenticationOptions.AuthorizationWellKnownConfiguration
                }, _smsAuthenticationOptions.ScimBaseUrl, true);
            }
            else
            {
                await _userActions.AddUser(record, null, null, false);
            }
            
            return await _resourceOwnerRepository.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber);
        }
    }
}