using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.UserManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.UserManagement.Extensions
{
    internal static class MappingExtensions
    {
        public static UpdateUserParameter ToParameter(this UpdateResourceOwnerViewModel updateResourceOwnerViewModel)
        {
            if (updateResourceOwnerViewModel == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerViewModel));
            }

            var result = new UpdateUserParameter
            {
                Password = updateResourceOwnerViewModel.NewPassword,
                TwoFactorAuthentication = updateResourceOwnerViewModel.TwoAuthenticationFactor,
                Claims = new List<Claim>(),
                Login = updateResourceOwnerViewModel.Name
            };
            if (!string.IsNullOrWhiteSpace(updateResourceOwnerViewModel.Name))
            {
                result.Claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name, updateResourceOwnerViewModel.Name));
            }

            if (!string.IsNullOrWhiteSpace(updateResourceOwnerViewModel.Email))
            {
                result.Claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email, updateResourceOwnerViewModel.Email));
            }

            if (!string.IsNullOrWhiteSpace(updateResourceOwnerViewModel.PhoneNumber))
            {
                result.Claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, updateResourceOwnerViewModel.PhoneNumber));
            }

            return result;
        }

        public static AddUserParameter ToAddUserParameter(this UpdateResourceOwnerViewModel updateResourceOwnerViewModel)
        {
            if (updateResourceOwnerViewModel == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerViewModel));
            }

            var result = new AddUserParameter
            {
                Login = updateResourceOwnerViewModel.Login,
                Password = updateResourceOwnerViewModel.NewPassword
            };

            if (!string.IsNullOrWhiteSpace(updateResourceOwnerViewModel.Name))
            {
                result.Claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name, updateResourceOwnerViewModel.Name));
            }

            if (!string.IsNullOrWhiteSpace(updateResourceOwnerViewModel.Email))
            {
                result.Claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email, updateResourceOwnerViewModel.Email));
            }

            if (!string.IsNullOrWhiteSpace(updateResourceOwnerViewModel.PhoneNumber))
            {
                result.Claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, updateResourceOwnerViewModel.PhoneNumber));
            }

            return result;
        }
    }
}
