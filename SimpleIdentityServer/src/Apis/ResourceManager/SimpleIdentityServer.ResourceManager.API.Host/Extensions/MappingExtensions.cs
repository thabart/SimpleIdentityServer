using SimpleIdentityServer.ResourceManager.API.Host.DTOs;
using SimpleIdentityServer.ResourceManager.Core.Models;
using System;

namespace SimpleIdentityServer.ResourceManager.API.Host.Extensions
{
    internal static class MappingExtensions
    {
        public static ProfileResponse ToDto(this ProfileAggregate profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            return new ProfileResponse
            {
                AuthUrl = profile.AuthUrl,
                OpenidUrl = profile.OpenidUrl,
                ScimUrl = profile.ScimUrl
            };
        }

        public static ProfileAggregate ToParameter(this ProfileResponse profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            return new ProfileAggregate
            {
                AuthUrl = profile.AuthUrl,
                OpenidUrl = profile.OpenidUrl,
                ScimUrl = profile.ScimUrl
            };
        }
    }
}
