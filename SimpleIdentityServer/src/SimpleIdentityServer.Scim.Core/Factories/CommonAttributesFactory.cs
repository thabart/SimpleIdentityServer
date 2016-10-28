#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Scim.Core.Factories
{
    public interface ICommonAttributesFactory
    {
        JProperty CreateIdJson(Representation representation);
        JProperty CreateIdJson(string id);
        RepresentationAttribute CreateId(Representation representation);
        IEnumerable<JProperty> CreateMetaDataAttributeJson(Representation representation, string location);
        RepresentationAttribute CreateMetaDataAttribute(Representation representation, string location);
        string GetFullPath(string key);
    }

    internal class CommonAttributesFactory : ICommonAttributesFactory
    {
        private readonly Dictionary<string, string> _mappingCommonAttrsKeysWithFullPath = new Dictionary<string, string>
        {
            {
                Constants.MetaResponseNames.ResourceType, Constants.ScimResourceNames.Meta + "." +Constants.MetaResponseNames.ResourceType
            },
            {
                Constants.MetaResponseNames.Version, Constants.ScimResourceNames.Meta + "." +Constants.MetaResponseNames.Version
            },
            {
                Constants.MetaResponseNames.Created, Constants.ScimResourceNames.Meta + "." +Constants.MetaResponseNames.Created
            },
            {
                Constants.MetaResponseNames.LastModified, Constants.ScimResourceNames.Meta + "." +Constants.MetaResponseNames.LastModified
            },
            {
                Constants.MetaResponseNames.Location, Constants.ScimResourceNames.Meta + "." +Constants.MetaResponseNames.Location
            },
            {
                Constants.ScimResourceNames.Meta, Constants.ScimResourceNames.Meta
            },
            {
                Constants.ScimResourceNames.Schemas, Constants.ScimResourceNames.Schemas
            },
            {
                Constants.IdentifiedScimResourceNames.ExternalId, Constants.IdentifiedScimResourceNames.ExternalId
            },
            {
                Constants.IdentifiedScimResourceNames.Id, Constants.IdentifiedScimResourceNames.Id
            }
        };
        private readonly ISchemaStore _schemaStore;

        public CommonAttributesFactory(ISchemaStore schemaStore)
        {
            _schemaStore = schemaStore;
        }

        public JProperty CreateIdJson(Representation representation)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            return CreateIdJson(representation.Id);
        }

        public JProperty CreateIdJson(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return new JProperty(Constants.IdentifiedScimResourceNames.Id, id);
        }

        public RepresentationAttribute CreateId(Representation representation)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            var commonAttrs = _schemaStore.GetCommonAttributes();
            var idAttr = commonAttrs.First(n => n.Name == Constants.IdentifiedScimResourceNames.Id);
            return new SingularRepresentationAttribute<string>(idAttr, representation.Id);
        }

        public RepresentationAttribute CreateMetaDataAttribute(Representation representation, string location)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentNullException(nameof(location));
            }

            var commonAttrs = _schemaStore.GetCommonAttributes();
            var metaAttr = commonAttrs.First(m => m.Name == Constants.ScimResourceNames.Meta) as ComplexSchemaAttributeResponse;
            return new ComplexRepresentationAttribute(metaAttr)
            {
                Values = new RepresentationAttribute[]
                {
                    new SingularRepresentationAttribute<string>(metaAttr.SubAttributes.First(a => a.Name == Constants.MetaResponseNames.ResourceType), representation.ResourceType),
                    new SingularRepresentationAttribute<DateTime>(metaAttr.SubAttributes.First(a => a.Name == Constants.MetaResponseNames.Created), representation.Created),
                    new SingularRepresentationAttribute<DateTime>(metaAttr.SubAttributes.First(a => a.Name == Constants.MetaResponseNames.LastModified), representation.LastModified),
                    new SingularRepresentationAttribute<string>(metaAttr.SubAttributes.First(a => a.Name == Constants.MetaResponseNames.Version), representation.Version),
                    new SingularRepresentationAttribute<string>(metaAttr.SubAttributes.First(a => a.Name == Constants.MetaResponseNames.Location), location)
                }
            };
        }

        public IEnumerable<JProperty> CreateMetaDataAttributeJson(Representation representation, string location)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentNullException(nameof(location));
            }

            return new JProperty[]
            {
                new JProperty(Constants.MetaResponseNames.ResourceType, representation.ResourceType),
                new JProperty(Constants.MetaResponseNames.Created, representation.Created),
                new JProperty(Constants.MetaResponseNames.LastModified, representation.LastModified),
                new JProperty(Constants.MetaResponseNames.Version, representation.Version),
                new JProperty(Constants.MetaResponseNames.Location, location)
            };
        }

        public string GetFullPath(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!_mappingCommonAttrsKeysWithFullPath.ContainsKey(key))
            {
                return null;
            }

            return _mappingCommonAttrsKeysWithFullPath[key];
        }
    }
}
