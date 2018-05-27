#region copyright
// Copyright 2016 Habart Thierry
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

using SimpleIdentityServer.Scim.Db.EF;
using SimpleIdentityServer.Scim.Db.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Scim.Startup.Extensions
{
    public static class ScimDbContextExtensions
    {
        private const char _separator = ',';
        private static string _id = Guid.NewGuid().ToString();
        private static string _externalId = Guid.NewGuid().ToString();
        private static string _adrId = Guid.NewGuid().ToString();
        private static string _localeId = Guid.NewGuid().ToString();
        private static string _ageId = Guid.NewGuid().ToString();
        private static string _genderId = Guid.NewGuid().ToString();
        private static string _ethnicityId = Guid.NewGuid().ToString();
        private static string _birthDateId = Guid.NewGuid().ToString();
        private static string _locationId = Guid.NewGuid().ToString();

        public static void EnsureSeedData(this ScimDbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            InsertSchemas(context);
            InsertRepresentations(context);
            try
            {
                context.SaveChanges();
            }
            catch { }
        }

        private static void InsertSchemas(ScimDbContext context)
        {
            if (!context.Schemas.Any())
            {
                context.Schemas.Add(UserSchema);
                context.Schemas.Add(GroupSchema);
                try
                {
                    context.SaveChanges();
                }
                catch
                {
                    // Trace.WriteLine("duplicated keys");
                }
            }
        }

        private static void InsertRepresentations(ScimDbContext context)
        {
            if (!context.Representations.Any())
            {
                context.Representations.AddRange(new[]
                {
                    new Representation
                    {
                        Id = "7d79392f-8a02-494c-949e-723a4db8ed16",
                        Version = "117ee9e4-e519-4ce6-b748-9691f70b43ce",
                        Created = DateTime.UtcNow,
                        LastModified = DateTime.UtcNow,
                        ResourceType = Common.Constants.ResourceTypes.User,
                        Attributes = new List<RepresentationAttribute>
                        {
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _id,
                                Value = "7d79392f-8a02-494c-949e-723a4db8ed16"
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _externalId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _adrId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _localeId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _ageId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _genderId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _ethnicityId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _birthDateId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _locationId
                            }
                        }
                    },
                    new Representation
                    {
                        Id = "c41c9e28-a4f8-447d-b170-f99563257d15",
                        Version = "b424ab6d-244d-4fa5-b4af-a27a23665996",
                        Created = DateTime.UtcNow,
                        LastModified = DateTime.UtcNow,
                        ResourceType = Common.Constants.ResourceTypes.User,
                        Attributes = new List<RepresentationAttribute>
                        {
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _id,
                                Value = "c41c9e28-a4f8-447d-b170-f99563257d15"
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _externalId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _adrId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _localeId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _ageId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _genderId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _ethnicityId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _birthDateId
                            },
                            new RepresentationAttribute
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = Common.Constants.SchemaAttributeTypes.String,
                                SchemaAttributeId = _locationId
                            }
                        }
                    }
                });
            }
        }

        private static class SchemaAttributeFactory
        {
            public static SchemaAttribute CreateAttributeWithId(
                string id,
                string name,
                string description,
                string type = Common.Constants.SchemaAttributeTypes.String,
                string mutability = Common.Constants.SchemaAttributeMutability.ReadWrite,
                string returned = Common.Constants.SchemaAttributeReturned.Default,
                string uniqueness = Common.Constants.SchemaAttributeUniqueness.None,
                bool caseExact = false,
                bool required = false,
                bool multiValued = false,
                string[] referenceTypes = null,
                string[] canonicalValues = null,
                bool isCommon = false)
            {
                return new SchemaAttribute
                {
                    Id = id,
                    Name = name,
                    Type = type,
                    MultiValued = multiValued,
                    Description = description,
                    Required = required,
                    CaseExact = caseExact,
                    Mutability = mutability,
                    Returned = returned,
                    Uniqueness = uniqueness,
                    ReferenceTypes = ConcatList(referenceTypes),
                    CanonicalValues = ConcatList(canonicalValues),
                    IsCommon = isCommon
                };
            }

            public static SchemaAttribute CreateAttribute(
                string name,
                string description,
                string type = Common.Constants.SchemaAttributeTypes.String,
                string mutability = Common.Constants.SchemaAttributeMutability.ReadWrite,
                string returned = Common.Constants.SchemaAttributeReturned.Default,
                string uniqueness = Common.Constants.SchemaAttributeUniqueness.None,
                bool caseExact = false,
                bool required = false,
                bool multiValued = false,
                string[] referenceTypes = null,
                string[] canonicalValues = null,
                bool isCommon = false)
            {
                return CreateAttributeWithId(Guid.NewGuid().ToString(), name, description, type, mutability, returned, uniqueness, caseExact, required, multiValued, referenceTypes, canonicalValues, isCommon);
            }

            public static SchemaAttribute CreateComplexAttribute(
                string id,
                string name,
                string description,
                List<SchemaAttribute> subAttributes,
                string type = Common.Constants.SchemaAttributeTypes.String,
                bool multiValued = false,
                bool required = false,
                bool caseExact = false,
                string mutability = Common.Constants.SchemaAttributeMutability.ReadWrite,
                string returned = Common.Constants.SchemaAttributeReturned.Default,
                string uniqueness = Common.Constants.SchemaAttributeUniqueness.None,
                string[] referenceTypes = null,
                string[] canonicalValues = null,
                bool isCommon = false)
            {
                return new SchemaAttribute
                {
                    Id = id,
                    Name = name,
                    MultiValued = multiValued,
                    Description = description,
                    Required = required,
                    CaseExact = caseExact,
                    Mutability = mutability,
                    Returned = returned,
                    Uniqueness = uniqueness,
                    ReferenceTypes = ConcatList(referenceTypes),
                    CanonicalValues = ConcatList(canonicalValues),
                    Children = subAttributes,
                    IsCommon = isCommon,
                    Type = Common.Constants.SchemaAttributeTypes.Complex
                };
            }

            public static SchemaAttribute CreateValueAttribute(
                string description,
                string[] referenceTypes = null,
                string type = Common.Constants.SchemaAttributeTypes.String,
                string mutability = Common.Constants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                        Common.Constants.MultiValueAttributeNames.Value,
                        description,
                        type: type,
                        referenceTypes: referenceTypes,
                        mutability: mutability);
            }

            public static SchemaAttribute CreateDisplayAttribute(
                string description,
                string mutability = Common.Constants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                        Common.Constants.MultiValueAttributeNames.Display,
                        description,
                        mutability: mutability);
            }

            public static SchemaAttribute CreateTypeAttribute(
                string description,
                string[] canonicalValues,
                string mutability = Common.Constants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                    Common.Constants.MultiValueAttributeNames.Type,
                    description,
                    canonicalValues: canonicalValues,
                    mutability: mutability);
            }

            public static SchemaAttribute CreatePrimaryAttribute(
                string description,
                string mutability = Common.Constants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                    Common.Constants.MultiValueAttributeNames.Primary,
                    description,
                    type: Common.Constants.SchemaAttributeTypes.Boolean,
                    mutability: mutability);
            }

            public static SchemaAttribute CreateRefAttribute(
                string description,
                string[] referenceTypes,
                string mutability = Common.Constants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                    Common.Constants.MultiValueAttributeNames.Ref,
                    description,
                    type: Common.Constants.SchemaAttributeTypes.Reference,
                    referenceTypes: referenceTypes,
                    mutability: mutability);
            }
        }

        private static List<SchemaAttribute> UserMetaDataAttributes = new List<SchemaAttribute>
        {
            SchemaAttributeFactory.CreateAttribute(Common.Constants.MetaResponseNames.ResourceType, "Name of the resource type of the resource", mutability: Common.Constants.SchemaAttributeMutability.ReadOnly, caseExact: true),
            SchemaAttributeFactory.CreateAttribute(Common.Constants.MetaResponseNames.Created, "The 'DateTime' that the resource was added to the service provider", mutability: Common.Constants.SchemaAttributeMutability.ReadOnly, type: Common.Constants.SchemaAttributeTypes.DateTime),
            SchemaAttributeFactory.CreateAttribute(Common.Constants.MetaResponseNames.LastModified, "The most recent DateTime than the details of this resource were updated at the service provider", mutability: Common.Constants.SchemaAttributeMutability.ReadOnly, type: Common.Constants.SchemaAttributeTypes.DateTime),
            SchemaAttributeFactory.CreateAttribute(Common.Constants.MetaResponseNames.Location, "URI of the resource being returned", mutability: Common.Constants.SchemaAttributeMutability.ReadOnly),
            SchemaAttributeFactory.CreateAttribute(Common.Constants.MetaResponseNames.Version, "Version of the resource being returned", mutability: Common.Constants.SchemaAttributeMutability.ReadOnly, caseExact: true),
        };
        
        private static List<SchemaAttribute> GroupMetaDataAttributes = new List<SchemaAttribute>
        {
            SchemaAttributeFactory.CreateAttribute(Common.Constants.MetaResponseNames.ResourceType, "Name of the resource type of the resource", mutability: Common.Constants.SchemaAttributeMutability.ReadOnly, caseExact: true),
            SchemaAttributeFactory.CreateAttribute(Common.Constants.MetaResponseNames.Created, "The 'DateTime' that the resource was added to the service provider", mutability: Common.Constants.SchemaAttributeMutability.ReadOnly, type: Common.Constants.SchemaAttributeTypes.DateTime),
            SchemaAttributeFactory.CreateAttribute(Common.Constants.MetaResponseNames.LastModified, "The most recent DateTime than the details of this resource were updated at the service provider", mutability: Common.Constants.SchemaAttributeMutability.ReadOnly, type: Common.Constants.SchemaAttributeTypes.DateTime),
            SchemaAttributeFactory.CreateAttribute(Common.Constants.MetaResponseNames.Location, "URI of the resource being returned", mutability: Common.Constants.SchemaAttributeMutability.ReadOnly),
            SchemaAttributeFactory.CreateAttribute(Common.Constants.MetaResponseNames.Version, "Version of the resource being returned", mutability: Common.Constants.SchemaAttributeMutability.ReadOnly, caseExact: true),
        };

        #region User

        #region Attributes

        private static List<SchemaAttribute> UserAddressAttributes = new List<SchemaAttribute>
        {
             SchemaAttributeFactory.CreateAttribute(Common.Constants.AddressResponseNames.Formatted, "The full mailing address, formatted for display or use with a mailing label.  This attribute MAY contain newlines."),
             SchemaAttributeFactory.CreateAttribute(Common.Constants.AddressResponseNames.StreetAddress, "The full street address component, which may include house number, street name, P.O. box, and multi-line extended street address information.  This attribute MAY contain newlines."),
             SchemaAttributeFactory.CreateAttribute(Common.Constants.AddressResponseNames.Locality, "The city or locality component."),
             SchemaAttributeFactory.CreateAttribute(Common.Constants.AddressResponseNames.Region, "The state or region component."),
             SchemaAttributeFactory.CreateAttribute(Common.Constants.AddressResponseNames.PostalCode, "The zip code or postal code component."),
             SchemaAttributeFactory.CreateAttribute(Common.Constants.AddressResponseNames.Country, "The country name component."),
             SchemaAttributeFactory.CreateTypeAttribute("A label indicating the attribute's function, e.g., 'work' or 'home'.", new string[] { "work", "home", "other" })
        };

        private static List<SchemaAttribute> UserLocationAttributes = new List<SchemaAttribute>
        {
             SchemaAttributeFactory.CreateAttribute(Constants.AttributeNames.Latitude, "Latitude", type: Common.Constants.SchemaAttributeTypes.Decimal),
             SchemaAttributeFactory.CreateAttribute(Constants.AttributeNames.Longitutde, "Longitutde", type: Common.Constants.SchemaAttributeTypes.Decimal),
        };

        #endregion

        private static Schema UserSchema = new Schema
        {
            Id = Common.Constants.SchemaUrns.User,
            Name = Common.Constants.ResourceTypes.User,
            Description = "User Account",
            Attributes = new List<SchemaAttribute>
            {
                // locale
                SchemaAttributeFactory.CreateAttributeWithId(
                    _localeId,
                     Common.Constants.UserResourceResponseNames.Locale,
                     "Used to indicate the User's default location"+
                                    "for purposes of localizing items such as currency, date time format, or"+
                                    "numerical representations."),
                // Age
                SchemaAttributeFactory.CreateAttributeWithId(
                    _ageId,
                     Constants.AttributeNames.Age,
                     "Age of the user",
                     type: Common.Constants.SchemaAttributeTypes.Integer),
                // Gender
                SchemaAttributeFactory.CreateAttributeWithId(
                    _genderId,
                     Constants.AttributeNames.Gender,
                     "Possible values (male, female, unknown)",
                     type: Common.Constants.SchemaAttributeTypes.String,
                     canonicalValues: new string[] { "male", "female", "unknown" }),
                // Ethnicity
                SchemaAttributeFactory.CreateAttributeWithId(
                    _ethnicityId,
                    Constants.AttributeNames.Ethnicity,
                     "Ethnicity",
                     type: Common.Constants.SchemaAttributeTypes.String),
                // Ethnicity
                SchemaAttributeFactory.CreateAttributeWithId(
                    _birthDateId,
                     Constants.AttributeNames.BirthDate,
                     "BirthDate",
                     type: Common.Constants.SchemaAttributeTypes.DateTime),
                // Location
                SchemaAttributeFactory.CreateComplexAttribute(
                    _locationId,
                    Constants.AttributeNames.Location,
                     "BirthDate",
                     UserLocationAttributes,
                     type: Common.Constants.SchemaAttributeTypes.DateTime),
                // Addresses
                SchemaAttributeFactory.CreateComplexAttribute(
                    _adrId,
                    Common.Constants.UserResourceResponseNames.Addresses,
                    "A physical mailing address for this User. Canonical type values of 'work', 'home', and 'other'.  This attribute is a complex type with the following sub-attributes.",
                    UserAddressAttributes,
                    multiValued: true),
                SchemaAttributeFactory.CreateAttributeWithId(_id, 
                    Common.Constants.IdentifiedScimResourceNames.Id, 
                    "Unique identifier for a SCIM resource as defined by the service provider",
                    mutability: Common.Constants.SchemaAttributeMutability.ReadOnly, 
                    caseExact: true, 
                    returned: Common.Constants.SchemaAttributeReturned.Always, 
                    isCommon: true),
                SchemaAttributeFactory.CreateAttribute(Common.Constants.IdentifiedScimResourceNames.ExternalId, "Identifier as defined by the provisioning client", caseExact: true, mutability: Common.Constants.SchemaAttributeMutability.ReadWrite, required: false, isCommon: true),
                SchemaAttributeFactory.CreateComplexAttribute(Guid.NewGuid().ToString(), Common.Constants.ScimResourceNames.Meta, "Complex attribute contaning resource metadata", UserMetaDataAttributes, mutability: Common.Constants.SchemaAttributeMutability.ReadOnly, returned: Common.Constants.SchemaAttributeReturned.Default, isCommon: true)
            },
            Meta = new MetaData
            {
                Id = Guid.NewGuid().ToString(),
                ResourceType = "Schema",
                Location = Common.Constants.SchemaUrns.User
            }
        };

        #endregion

        #region Group

        private static List<SchemaAttribute> GroupMembersAttribute = new List<SchemaAttribute>
        {
            SchemaAttributeFactory.CreateAttribute(Common.Constants.GroupMembersResponseNames.Value, "Identifier of the member of this Group.", uniqueness: Common.Constants.SchemaAttributeUniqueness.None, required : false, mutability: Common.Constants.SchemaAttributeMutability.Immutable),
            SchemaAttributeFactory.CreateRefAttribute("The URI corresponding to a SCIM resource that is a member of this Group.", new string[] { "User", "Group" }, Common.Constants.SchemaAttributeMutability.Immutable),
            SchemaAttributeFactory.CreateTypeAttribute("A label indicating the type of resource, e.g., 'User' or 'Group'.", new string[] { "User", "Group" }, Common.Constants.SchemaAttributeMutability.Immutable)
        };

        private static Schema GroupSchema = new Schema
        {
            Id = Common.Constants.SchemaUrns.Group,
            Name = Common.Constants.ResourceTypes.Group,
            Description = "Group",
            Attributes = new List<SchemaAttribute>
            {
                SchemaAttributeFactory.CreateAttribute(Common.Constants.GroupResourceResponseNames.DisplayName, "A human-readable name for the Group. REQUIRED.", uniqueness: Common.Constants.SchemaAttributeUniqueness.None, required : false),
                SchemaAttributeFactory.CreateComplexAttribute(Guid.NewGuid().ToString(), Common.Constants.GroupResourceResponseNames.Members, "A list of members of the Group.", GroupMembersAttribute, multiValued: true),
                SchemaAttributeFactory.CreateAttribute(Common.Constants.IdentifiedScimResourceNames.Id, "Unique identifier for a SCIM resource as defined by the service provider", mutability: Common.Constants.SchemaAttributeMutability.ReadOnly, caseExact: true, returned: Common.Constants.SchemaAttributeReturned.Always, isCommon: true),
                SchemaAttributeFactory.CreateAttributeWithId(_externalId, Common.Constants.IdentifiedScimResourceNames.ExternalId, "Identifier as defined by the provisioning client", caseExact: true, mutability: Common.Constants.SchemaAttributeMutability.ReadWrite, required: false, isCommon: true),
                SchemaAttributeFactory.CreateComplexAttribute(Guid.NewGuid().ToString(), Common.Constants.ScimResourceNames.Meta, "Complex attribute contaning resource metadata", GroupMetaDataAttributes, mutability: Common.Constants.SchemaAttributeMutability.ReadOnly, returned: Common.Constants.SchemaAttributeReturned.Default, isCommon: true)
            },
            Meta = new MetaData
            {
                Id = Guid.NewGuid().ToString(),
                ResourceType = "Schema",
                Location = Common.Constants.SchemaUrns.Group
            }
        };

        #endregion

        private static string ConcatList(IEnumerable<string> lst)
        {
            if (lst == null)
            {
                return string.Empty;
            }

            return string.Join(_separator.ToString(), lst);
        }
    }
}
