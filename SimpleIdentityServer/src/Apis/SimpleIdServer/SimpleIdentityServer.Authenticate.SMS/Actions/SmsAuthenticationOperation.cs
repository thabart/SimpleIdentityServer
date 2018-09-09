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
    using Core.Jwt;

    public interface ISmsAuthenticationOperation
    {
        Task<ResourceOwner> Execute(string phoneNumber);
    }

    internal sealed class SmsAuthenticationOperation : ISmsAuthenticationOperation
    {
        private readonly IGenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IUserActions _userActions; 
        private readonly SmsAuthenticationOptions _smsAuthenticationOptions;

        public SmsAuthenticationOperation(IGenerateAndSendSmsCodeOperation generateAndSendSmsCodeOperation, IResourceOwnerRepository resourceOwnerRepository, IUserActions userActions, SmsAuthenticationOptions smsAuthenticationOptions)
        {
            _generateAndSendSmsCodeOperation = generateAndSendSmsCodeOperation;
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

            // 1. Send the confirmation code (SMS).
            await _generateAndSendSmsCodeOperation.Execute(phoneNumber).ConfigureAwait(false);
            // 2. Try to get the resource owner.
            var resourceOwner = await _resourceOwnerRepository.GetResourceOwnerByClaim(Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber).ConfigureAwait(false);
            if (resourceOwner != null)
            {
                return resourceOwner;
            }

            // 3. Create a new resource owner.
            var id = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber),
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified, "false")
            };
            var record = new AddUserParameter(id, Guid.NewGuid().ToString(), claims);
            // 3.1 Add scim resource.
            if (_smsAuthenticationOptions.IsScimResourceAutomaticallyCreated)
            {
                await _userActions.AddUser(record, new AuthenticationParameter
                {
                    ClientId = _smsAuthenticationOptions.AuthenticationOptions.ClientId,
                    ClientSecret = _smsAuthenticationOptions.AuthenticationOptions.ClientSecret,
                    WellKnownAuthorizationUrl = _smsAuthenticationOptions.AuthenticationOptions.AuthorizationWellKnownConfiguration
                }, _smsAuthenticationOptions.ScimBaseUrl, true).ConfigureAwait(false);
            }
            else
            {
                // 3.2 Add user.
                await _userActions.AddUser(record, null, null, false).ConfigureAwait(false);
            }
            
            return await _resourceOwnerRepository.GetResourceOwnerByClaim(Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber).ConfigureAwait(false);
        }
    }
}