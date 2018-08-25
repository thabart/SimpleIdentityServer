using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Requests;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Responses;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
                AttributeId = addMappingRequest.AttributeId,
                SchemaId = addMappingRequest.SchemaId
            };
        }

        public static AdConfiguration ToModel(this UpdateAdConfigurationRequest adConfigurationResponse)
        {
            if (adConfigurationResponse == null)
            {
                throw new ArgumentNullException(nameof(adConfigurationResponse));
            }

            return new AdConfiguration
            {
                DistinguishedName = adConfigurationResponse.DistinguishedName,
                IpAdr = adConfigurationResponse.IpAdr,
                IsEnabled = adConfigurationResponse.IsEnabled,
                Password = adConfigurationResponse.Password,
                Port = adConfigurationResponse.Port,
                Username = adConfigurationResponse.Username,
                AdConfigurationSchemas = adConfigurationResponse.Schemas == null ? new List<AdConfigurationSchema>() : 
                    adConfigurationResponse.Schemas.Select(s => new AdConfigurationSchema
                    {
                        Filter = s.Filter,
                        FilterClass = s.FilterClass,
                        SchemaId = s.SchemaId
                    })
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
                Username = adConfiguration.Username,
                Schemas = adConfiguration.AdConfigurationSchemas == null ? new List<AdConfigurationSchemaResponse>() :
                    adConfiguration.AdConfigurationSchemas.Select(s => new AdConfigurationSchemaResponse
                    {
                        Filter = s.Filter,
                        FilterClass = s.FilterClass,
                        SchemaId = s.SchemaId
                    })
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
                AttributeId = adMapping.AttributeId,
                SchemaId = adMapping.SchemaId
            };
        }
    }
}
