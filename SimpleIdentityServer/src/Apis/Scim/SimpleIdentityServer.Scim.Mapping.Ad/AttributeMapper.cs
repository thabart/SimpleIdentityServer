using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Mapping.Ad.Stores;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad
{
    internal sealed class AttributeMapper
    {
        private readonly IConfigurationStore _configurationStore;

        public AttributeMapper(IConfigurationStore configurationStore)
        {
            _configurationStore = configurationStore;
        }

        public async Task<string> Map(Representation representation, string attributeId)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            if(string.IsNullOrWhiteSpace(attributeId))
            {
                throw new ArgumentNullException(nameof(attributeId));
            }

            // var directoryEntry = new DirectoryEntry();
            var configuration = await _configurationStore.GetConfiguration();
            if (configuration.IsEnabled)
            {
                return null;
            }

            // var ldap = new DirectoryEntry("", "", "");
            // var directorySearch = new DirectorySearcher(ldap, "");
            // directorySearch.FindOne();

            // TODO : EVALUATE THE AD QUERY CN=$(userName)
            // TODO : GET THE MAPPING RULE
            // TODO : GET THE ATTRIBUTE VIA PROPERTY NAME
            return null;
        }
    }
}
