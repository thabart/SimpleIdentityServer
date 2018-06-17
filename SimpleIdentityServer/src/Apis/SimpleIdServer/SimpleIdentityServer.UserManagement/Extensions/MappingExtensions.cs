using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.UserManagement.ViewModels;
using System;

namespace SimpleIdentityServer.UserManagement.Extensions
{
    internal static class MappingExtensions
    {
        public static AddUserParameter ToAddUserParameter(this UpdateResourceOwnerCredentialsViewModel updateResourceOwnerViewModel)
        {
            if (updateResourceOwnerViewModel == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerViewModel));
            }

            var result = new AddUserParameter
            {
                Login = updateResourceOwnerViewModel.Login,
                Password = updateResourceOwnerViewModel.Password
            };

            return result;
        }
    }
}
