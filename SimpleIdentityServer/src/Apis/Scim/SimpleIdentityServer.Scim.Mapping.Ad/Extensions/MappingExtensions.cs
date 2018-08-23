using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Requests;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Responses;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using System;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Extensions
{
    internal static class MappingExtensions
    {
        public static AdMapping ToModel(this AddMappingRequest addMappingRequest)
        {
            if(addMappingRequest == null)
            {
                throw new ArgumentNullException(nameof(addMappingRequest));
            }

            return new AdMapping
            {
                AdPropertyName = addMappingRequest.AdPropertyName,
                AttributeId = addMappingRequest.AttributeId
            };
        }

        public static AdConfigurationResponse ToDto(this AdConfiguration adConfiguration)
        {
            if(adConfiguration == null)
            {
                throw new ArgumentNullException(nameof(adConfiguration));
            }

            return new AdConfigurationResponse
            {
                DistinguishedName = adConfiguration.DistinguishedName,
                IpAdr = adConfiguration.IpAdr,
                IsEnabled = adConfiguration.IsEnabled,
                Password = adConfiguration.Password,
                Port = adConfiguration.Port,
                UserFilter = adConfiguration.UserFilter,
                Username = adConfiguration.Username
            };
        }

        public static AddMappingRequest ToDto(this AdMapping adMapping)
        {
            if (adMapping == null)
            {
                throw new ArgumentNullException(nameof(adMapping));
            }

            return new AddMappingRequest
            {
                AdPropertyName = adMapping.AdPropertyName,
                AttributeId = adMapping.AttributeId
            };
        }
    }
}
