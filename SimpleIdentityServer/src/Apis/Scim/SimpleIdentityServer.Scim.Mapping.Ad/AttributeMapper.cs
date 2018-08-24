using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Mapping.Ad.Stores;
using System;
using System.DirectoryServices.Protocols;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad
{
    internal sealed class AttributeMapper : IAttributeMapper
    {
        private readonly IConfigurationStore _configurationStore;
        private readonly IMappingStore _mappingStore;
        private readonly IUserFilterParser _userFilterParser;

        public AttributeMapper(IConfigurationStore configurationStore, IMappingStore mappingStore, IUserFilterParser userFilterParser)
        {
            _configurationStore = configurationStore;
            _mappingStore = mappingStore;
            _userFilterParser = userFilterParser;
        }

        public async Task<Representation> Map(Representation representation)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            var configuration = _configurationStore.GetConfiguration();
            if (configuration == null || configuration.IsEnabled)
            {
                return representation;
            }

            var userFilter = _userFilterParser.Parse(configuration.UserFilter, representation);
            if(userFilter == null)
            {
                return representation;
            }

            using (var ldapHelper = new LdapHelper())
            {
                ldapHelper.Connect(configuration.IpAdr, configuration.Port, configuration.Username, configuration.Password);
                var searchResponse = ldapHelper.Search(configuration.DistinguishedName, userFilter);
                if (searchResponse.Entries.Count != 1)
                {
                    return representation;
                }

                var ldapUser = searchResponse.Entries[0];
                foreach(var attr in representation.Attributes)
                {
                    var attributeId = attr.SchemaAttribute.Id;
                    var attributeMapping = await _mappingStore.GetMapping(attributeId).ConfigureAwait(false);
                    if (attributeMapping == null)
                    {
                        continue;
                    }

                    if (ldapUser.Attributes.Contains(attributeMapping.AdPropertyName))
                    {
                        var a = ldapUser.Attributes[attributeMapping.AdPropertyName];
                        attr.Value = TryGetAttributeValueAsString(a);
                    }
                }
            }

            return representation;
        }

        private static string TryGetAttributeValueAsString(DirectoryAttribute attr)
        {
            string value = null;
            if (attr != null && attr.Count > 0)
            {
                value = attr[0] as string;
            }

            return value;
        }
    }
}
