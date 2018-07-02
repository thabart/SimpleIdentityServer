using SimpleIdentityServer.Authenticate.Basic.ViewModels;
using SimpleIdentityServer.Core.Parameters;
using System;

namespace SimpleIdentityServer.Authenticate.Basic.Extensions
{
    public static class MappingExtensions
    {
        /*
        public static LocalAuthenticationParameter ToParameter(this AuthorizeViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            return new LocalAuthenticationParameter
            {
                UserName = viewModel.UserName,
                Password = viewModel.Password
            };
        }

        public static LocalAuthenticationParameter ToParameter(this AuthorizeOpenIdViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            return new LocalAuthenticationParameter
            {
                UserName = viewModel.UserName,
                Password = viewModel.Password
            };
        }
        */
    }
}
