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

            return new AddUserParameter(updateResourceOwnerViewModel.Login, updateResourceOwnerViewModel.Password);
        }
    }
}
