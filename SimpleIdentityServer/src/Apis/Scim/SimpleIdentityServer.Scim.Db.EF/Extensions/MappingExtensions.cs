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

using SimpleIdentityServer.Scim.Common.DTOs;
using System;
using System.Collections.Generic;
using Domain = SimpleIdentityServer.Scim.Common.Models;
using Model = SimpleIdentityServer.Scim.Db.EF.Models;

namespace SimpleIdentityServer.Scim.Db.EF.Extensions
{
    internal static class MappingExtensions
    {
        private const char _separator = ',';

        public static Domain.Representation ToDomain(this Model.Representation representation)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            return new Domain.Representation
            {
                Id = representation.Id,
                Created = representation.Created,
                LastModified = representation.Created,
                ResourceType = representation.ResourceType,
                Version = representation.Version
            };
        }

        public static Model.Representation SetData(this Model.Representation representation, Domain.Representation repr)
        {
            if (repr == null)
            {
                throw new ArgumentNullException(nameof(repr));
            }

            representation.Created = repr.Created;
            representation.LastModified = repr.LastModified;
            representation.ResourceType = repr.ResourceType;
            representation.Version = repr.Version;
            return representation;
        }

        public static SchemaAttributeResponse ToDomain(this Model.SchemaAttribute attr)
        {
            if (attr == null)
            {
                throw new ArgumentNullException(nameof(attr));
            }

            var result = new SchemaAttributeResponse();
            SetData(result, attr);
            return result;
        }

        public static void SetData(this SchemaAttributeResponse resp, Model.SchemaAttribute attr)
        {
            resp.Id = attr.Id;
            resp.CaseExact = attr.CaseExact;
            resp.Description = attr.Description;
            resp.MultiValued = attr.MultiValued;
            resp.Name = attr.Name;
            resp.Required = attr.Required;
            resp.Mutability = attr.Mutability;
            resp.Returned = attr.Returned;
            resp.Type = attr.Type;
            resp.Uniqueness = attr.Uniqueness;
            resp.CanonicalValues = SplitList(attr.CanonicalValues);
            resp.ReferenceTypes = SplitList(attr.ReferenceTypes);
        }

        public static SchemaResponse ToDomain(this Model.Schema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            return new SchemaResponse
            {
                Id = schema.Id,
                Description = schema.Description,
                Name = schema.Name
            };
        }

        public static Meta ToDomain(this Model.MetaData meta)
        {
            if (meta == null)
            {
                throw new ArgumentNullException(nameof(meta));
            }

            return new Meta
            {
                ResourceType = meta.ResourceType,
                Location = meta.Location 
            };
        }

        public static Model.SchemaAttribute ToModel(this SchemaAttributeResponse attr)
        {
            if (attr == null)
            {
                throw new ArgumentNullException(nameof(attr));
            }

            return new Model.SchemaAttribute
            {
                CaseExact = attr.CaseExact,
                Description = attr.Description,
                Id = attr.Id,
                MultiValued = attr.MultiValued,
                Mutability = attr.Mutability,
                Name = attr.Name,
                Required = attr.Required,
                Returned = attr.Returned,
                Uniqueness = attr.Uniqueness,
                Type = attr.Type,
                CanonicalValues = ConcatList(attr.CanonicalValues),
                ReferenceTypes = ConcatList(attr.ReferenceTypes)
            };
        }

        public static IEnumerable<string> SplitList(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return new string[0];
            }

            return str.Split(_separator);
        }

        public static string ConcatList(IEnumerable<string> lst)
        {
            if (lst == null)
            {
                return string.Empty;
            }

            return string.Join(_separator.ToString(), lst);
        }
    }
}
