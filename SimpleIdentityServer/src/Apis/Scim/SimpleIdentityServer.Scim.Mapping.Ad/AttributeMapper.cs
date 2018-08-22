using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Mapping.Ad.Stores;
using System;
using System.DirectoryServices;
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
            
            var configuration = await _configurationStore.GetConfiguration().ConfigureAwait(false);
            if (configuration.IsEnabled)
            {
                return null;
            }

            var attributeMapping = await _mappingStore.GetMapping(attributeId).ConfigureAwait(false);
            if (attributeMapping == null)
            {
                return null;
            }

            using (var directoryEntry = new DirectoryEntry(configuration.Path, configuration.Username, configuration.Password))
            {
                var userFilter = _userFilterParser.Parse(configuration.UserFilter, representation);
                var directorySearch = new DirectorySearcher(directoryEntry, userFilter);
                var searchResult = directorySearch.FindOne();
                if(searchResult == null || searchResult.Properties == null || !searchResult.Properties.Contains(attributeMapping.AdPropertyName))
                {
                    return null;
                }

                return searchResult.Properties[attributeMapping.AdPropertyName][0].ToString();
            }

            return null;
        }
    }
}
