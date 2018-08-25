using SimpleIdentityServer.Scim.Common.Models;
using SimpleIdentityServer.Scim.Mapping.Ad.Extensions;
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

        public async Task Map(Representation representation, string schemaId)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            if (string.IsNullOrWhiteSpace(schemaId))
            {
                throw new ArgumentNullException(nameof(schemaId));
            }

            var configuration = _configurationStore.GetConfiguration();
            if (configuration == null || configuration.IsEnabled)
            {
                return;
            }

            var userFilter = _userFilterParser.Parse(configuration.UserFilter, representation);
            if(userFilter == null)
            {
                return;
            }

            using (var ldapHelper = new LdapHelper())
            {
                ldapHelper.Connect(configuration.IpAdr, configuration.Port, configuration.Username, configuration.Password);
                var searchResponse = ldapHelper.Search(configuration.DistinguishedName, userFilter);
                if (searchResponse.Entries.Count != 1)
                {
                    return;
                }

                var ldapUser = searchResponse.Entries[0];
                foreach(var attr in representation.Attributes)
                {
                    var attributeId = attr.SchemaAttribute.Id;
                    var attributeMapping = await _mappingStore.GetMapping(attributeId).ConfigureAwait(false);
                    if (attributeMapping == null && attributeMapping.SchemaId != schemaId)
                    {
                        continue;
                    }

                    if (ldapUser.Attributes.Contains(attributeMapping.AdPropertyName))
                    {
                        var ldapAttr = ldapUser.Attributes[attributeMapping.AdPropertyName];
                        SetValue(attr, ldapAttr);
                    }
                }
            }
        }

        private static void SetValue(RepresentationAttribute representationAttr, DirectoryAttribute ldapAttr)
        {
            if (representationAttr.SchemaAttribute == null)
            {
                return;
            }

            var ldapAttrValue = ldapAttr.TryGetAttributeValueAsString();
            switch (representationAttr.SchemaAttribute.Type)
            {
                case Scim.Common.Constants.SchemaAttributeTypes.String:
                    var tmp = (SingularRepresentationAttribute<string>)representationAttr;
                    tmp.Value = ldapAttrValue;
                    break;
                case Scim.Common.Constants.SchemaAttributeTypes.Boolean:
                    var b = false;
                    bool.TryParse(ldapAttrValue, out b);
                    var tmp1 = (SingularRepresentationAttribute<bool>)representationAttr;
                    tmp1.Value = b;
                    break;
                case Scim.Common.Constants.SchemaAttributeTypes.DateTime:
                    DateTime d = default(DateTime);
                    DateTime.TryParse(ldapAttrValue, out d);
                    var tmp2 = (SingularRepresentationAttribute<DateTime>)representationAttr;
                    tmp2.Value = d;
                    break;
                case Scim.Common.Constants.SchemaAttributeTypes.Decimal:
                    decimal dec;
                    decimal.TryParse(ldapAttrValue, out dec);
                    var tmp3 = (SingularRepresentationAttribute<decimal>)representationAttr;
                    tmp3.Value = dec;
                    break;
                case Scim.Common.Constants.SchemaAttributeTypes.Integer:
                    int i;
                    int.TryParse(ldapAttrValue, out i);
                    var tmp4 = (SingularRepresentationAttribute<int>)representationAttr;
                    tmp4.Value = i;
                    break;
            }
        }
    }
}
