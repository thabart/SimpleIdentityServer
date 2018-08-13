using Microsoft.AspNetCore.Authentication;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.UserManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.UserManagement.Extensions
{
    internal static class MappingExtensions
    {
        public static AuthproviderResponse ToDto(this AuthenticationScheme authenticationScheme)
        {
            if(authenticationScheme == null)
            {
                throw new ArgumentNullException(nameof(authenticationScheme));
            }

            return new AuthproviderResponse
            {
                AuthenticationScheme = authenticationScheme.Name,
                DisplayName = authenticationScheme.DisplayName
            };
        }

        public static IEnumerable<AuthproviderResponse> ToDtos(this IEnumerable<AuthenticationScheme> authenticationSchemes)
        {
            if (authenticationSchemes == null)
            {
                throw new ArgumentNullException(nameof(authenticationSchemes));
            }

            return authenticationSchemes.Select(a => a.ToDto());
        }

        public static AddUserParameter ToAddUserParameter(this UpdateResourceOwnerCredentialsViewModel updateResourceOwnerViewModel)
        {
            if (updateResourceOwnerViewModel == null)
            {
                throw new ArgumentNullException(nameof(updateResourceOwnerViewModel));
            }

            return new AddUserParameter(updateResourceOwnerViewModel.Login, updateResourceOwnerViewModel.Password);
        }

        public static ProfileResponse ToDto(this ResourceOwnerProfile profile)
        {
            if(profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            return new ProfileResponse
            {
                Issuer = profile.Issuer,
                UserId = profile.Subject,
                CreateDateTime = profile.CreateDateTime,
                UpdateTime = profile.UpdateTime                
            };
        }
    }
}
