using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Services;
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
        private readonly IGenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IUserActions _userActions; 

        public SmsAuthenticationOperation(IGenerateAndSendSmsCodeOperation generateAndSendSmsCodeOperation, IResourceOwnerRepository resourceOwnerRepository, IUserActions userActions, ISubjectBuilder subjectBuilder,
            SmsAuthenticationOptions smsAuthenticationOptions)
        {
            _generateAndSendSmsCodeOperation = generateAndSendSmsCodeOperation;
            _resourceOwnerRepository = resourceOwnerRepository;
            _userActions = userActions;
        }

        public async Task<ResourceOwner> Execute(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            // 1. Send the confirmation code (SMS).
            await _generateAndSendSmsCodeOperation.Execute(phoneNumber);
            // 2. Try to get the resource owner.
            var resourceOwner = await _resourceOwnerRepository.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber);
            if (resourceOwner != null)
            {
                return resourceOwner;
            }

            // 3. Create a new resource owner.
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber),
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified, "false")
            };
            var record = new AddUserParameter(Guid.NewGuid().ToString(), claims);
            await _userActions.AddUser(record).ConfigureAwait(false);            
            return await _resourceOwnerRepository.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber);
        }
    }
}