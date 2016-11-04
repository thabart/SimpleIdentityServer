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

using SimpleIdentityServer.Scim.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Scim.Core.Stores
{
    public interface ISchemaStore
    {
        IEnumerable<SchemaResponse> GetSchemas();
        SchemaResponse GetSchema(string id);
        IEnumerable<SchemaAttributeResponse> GetCommonAttributes();
    }

    internal class SchemaStore : ISchemaStore
    {
        private static class SchemaAttribute
        {
            public static SchemaAttributeResponse CreateAttribute(
                string name,
                string description,
                string type = Constants.SchemaAttributeTypes.String,
                string mutability = Constants.SchemaAttributeMutability.ReadWrite,
                string returned = Constants.SchemaAttributeReturned.Default,
                string uniqueness = Constants.SchemaAttributeUniqueness.None,
                bool caseExact = false,
                bool required = false,
                bool multiValued = false,
                string[] referenceTypes = null,
                string[] canonicalValues = null)
            {
                return new SchemaAttributeResponse
                {
                    Name = name,
                    Type = type,
                    MultiValued = multiValued,
                    Description = description,
                    Required = required,
                    CaseExact = caseExact,
                    Mutability = mutability,
                    Returned = returned,
                    Uniqueness = uniqueness,
                    ReferenceTypes = referenceTypes,
                    CanonicalValues = canonicalValues
                };
            }

            public static SchemaAttributeResponse CreateComplexAttribute(
                string name,
                string description,
                IEnumerable<SchemaAttributeResponse> subAttributes,
                string type = Constants.SchemaAttributeTypes.String,
                bool multiValued = false,
                bool required = false,
                bool caseExact = false,
                string mutability = Constants.SchemaAttributeMutability.ReadWrite,
                string returned = Constants.SchemaAttributeReturned.Default,
                string uniqueness = Constants.SchemaAttributeUniqueness.None,
                string[] referenceTypes = null,
                string[] canonicalValues = null)
            {
                return new ComplexSchemaAttributeResponse
                {
                    Name = name,
                    MultiValued = multiValued,
                    Description = description,
                    Required = required,
                    CaseExact = caseExact,
                    Mutability = mutability,
                    Returned = returned,
                    Uniqueness = uniqueness,
                    ReferenceTypes = referenceTypes,
                    CanonicalValues = canonicalValues,
                    SubAttributes = subAttributes
                };
            }

            public static SchemaAttributeResponse CreateValueAttribute(
                string description,
                string[] referenceTypes = null,
                string type = Constants.SchemaAttributeTypes.String,
                string mutability = Constants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                        Constants.MultiValueAttributeNames.Value,
                        description,
                        type: type,
                        referenceTypes: referenceTypes,
                        mutability: mutability);
            }

            public static SchemaAttributeResponse CreateDisplayAttribute(
                string description,
                string mutability = Constants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                        Constants.MultiValueAttributeNames.Display,
                        description,
                        mutability: mutability);
            }

            public static SchemaAttributeResponse CreateTypeAttribute(
                string description,
                string[] canonicalValues,
                string mutability = Constants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                    Constants.MultiValueAttributeNames.Type,
                    description,
                    canonicalValues: canonicalValues,
                    mutability: mutability);
            }

            public static SchemaAttributeResponse CreatePrimaryAttribute(
                string description,
                string mutability = Constants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                    Constants.MultiValueAttributeNames.Primary,
                    description,
                    type: Constants.SchemaAttributeTypes.Boolean,
                    mutability: mutability);
            }

            public static SchemaAttributeResponse CreateRefAttribute(
                string description,
                string[] referenceTypes,
                string mutability = Constants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                    Constants.MultiValueAttributeNames.Ref,
                    description,
                    type: Constants.SchemaAttributeTypes.Reference,
                    referenceTypes: referenceTypes,
                    mutability: mutability);
            }
        }

        private static IEnumerable<SchemaAttributeResponse> MetaDataAttributes = new SchemaAttributeResponse[]
        {
            SchemaAttribute.CreateAttribute(Constants.MetaResponseNames.ResourceType, "Name of the resource type of the resource", mutability: Constants.SchemaAttributeMutability.ReadOnly, caseExact: true),
            SchemaAttribute.CreateAttribute(Constants.MetaResponseNames.Created, "The 'DateTime' that the resource was added to the service provider", mutability: Constants.SchemaAttributeMutability.ReadOnly, type: Constants.SchemaAttributeTypes.DateTime),
            SchemaAttribute.CreateAttribute(Constants.MetaResponseNames.LastModified, "The most recent DateTime than the details of this resource were updated at the service provider", mutability: Constants.SchemaAttributeMutability.ReadOnly, type: Constants.SchemaAttributeTypes.DateTime),
            SchemaAttribute.CreateAttribute(Constants.MetaResponseNames.Location, "URI of the resource being returned", mutability: Constants.SchemaAttributeMutability.ReadOnly),
            SchemaAttribute.CreateAttribute(Constants.MetaResponseNames.Version, "Version of the resource being returned", mutability: Constants.SchemaAttributeMutability.ReadOnly, caseExact: true),
        };

        private static IEnumerable<SchemaAttributeResponse> CommonAttributes = new SchemaAttributeResponse[]
        {
            SchemaAttribute.CreateAttribute(Constants.IdentifiedScimResourceNames.Id, "Unique identifier for a SCIM resource as defined by the service provider", mutability: Constants.SchemaAttributeMutability.ReadOnly, caseExact: true, returned: Constants.SchemaAttributeReturned.Always),
            SchemaAttribute.CreateAttribute(Constants.IdentifiedScimResourceNames.ExternalId, "Identifier as defined by the provisioning client", caseExact: true, mutability: Constants.SchemaAttributeMutability.ReadWrite, required: false),
            SchemaAttribute.CreateComplexAttribute(Constants.ScimResourceNames.Meta, "Complex attribute contaning resource metadata", MetaDataAttributes, mutability: Constants.SchemaAttributeMutability.ReadOnly, returned: Constants.SchemaAttributeReturned.Default)
        };

        #region User

        #region Attributes

        private static IEnumerable<SchemaAttributeResponse> EmailAttributeSub = new SchemaAttributeResponse[]
        {
           SchemaAttribute.CreateValueAttribute("Email addresses for the user.  The value SHOULD be canonicalized by the service provider, e.g., 'bjensen@example.com' instead of 'bjensen@EXAMPLE.COM'. Canonical type values of 'work', 'home', and 'other'."),
           SchemaAttribute.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY."),
           SchemaAttribute.CreateTypeAttribute("A label indicating the attribute's function, e.g., 'work' or 'home'.", new string[] { "work", "home", "other" }),
           SchemaAttribute.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute, e.g., the preferred mailing address or primary email address. The primary attribute value 'true' MUST appear no more than once.")
        };

        private static IEnumerable<SchemaAttributeResponse> UserPhoneNumberAttributes = new SchemaAttributeResponse[]
        {
           SchemaAttribute.CreateValueAttribute("Phone number of the User."),
           SchemaAttribute.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY."),
           SchemaAttribute.CreateTypeAttribute("A label indicating the attribute's function, e.g., 'work', 'home', 'mobile'.", new string[] { "work", "home", "mobile", "fax", "pager", "other" }),
           SchemaAttribute.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute, e.g., the preferred phone number or primary phone number.  The primary attribute value 'true' MUST appear no more than once.")
        };

        private static IEnumerable<SchemaAttributeResponse> UserImsAttributes = new SchemaAttributeResponse[]
        {
            SchemaAttribute.CreateValueAttribute("Instant messaging address for the User."),
            SchemaAttribute.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.READ - ONLY."),
            SchemaAttribute.CreateTypeAttribute("A label indicating the attribute's function, e.g., 'aim', 'gtalk', 'xmpp'.", new string[] { "aim", "gtalk", "icq", "xmpp", "msn", "skype", "qq", "yahoo" }),
            SchemaAttribute.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute, e.g., the preferred messenger or primary messenger. The primary attribute value 'true' MUST appear no more than once."),
        };

        private static IEnumerable<SchemaAttributeResponse> UserPhotoAttributes = new SchemaAttributeResponse[]
        {
            SchemaAttribute.CreateValueAttribute("URL of a photo of the User.", referenceTypes: new string [] { "external" }, type: Constants.SchemaAttributeTypes.Reference),
            SchemaAttribute.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY."),
            SchemaAttribute.CreateTypeAttribute("A label indicating the attribute's function, i.e., 'photo' or 'thumbnail'.", new string[] { "photo", "thumbnail" }),
            SchemaAttribute.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute, e.g., the preferred photo or thumbnail.  The primary attribute value 'true' MUST appear no more than once.")
        };

        private static IEnumerable<SchemaAttributeResponse> UserGroupAttributes = new SchemaAttributeResponse[]
        {
            SchemaAttribute.CreateValueAttribute("The identifier of the User's group.", mutability: Constants.SchemaAttributeMutability.ReadOnly),
            SchemaAttribute.CreateRefAttribute("The URI of the corresponding 'Group' resource to which the user belongs.", new string[] { "User", "Group" }, mutability: Constants.SchemaAttributeMutability.ReadOnly),
            SchemaAttribute.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY.", mutability: Constants.SchemaAttributeMutability.ReadOnly),
            SchemaAttribute.CreateTypeAttribute("A label indicating the attribute's function, e.g., 'direct' or 'indirect'.", new string[] { "direct", "indirect" }, mutability: Constants.SchemaAttributeMutability.ReadOnly),
        };

        private static IEnumerable<SchemaAttributeResponse> UserEntitlementAttributes = new SchemaAttributeResponse[]
        {
            SchemaAttribute.CreateValueAttribute("The value of an entitlement."),
            SchemaAttribute.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY."),
            SchemaAttribute.CreateTypeAttribute("A label indicating the attribute's function.", new string[] { }),
            SchemaAttribute.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute.  The primary attribute value 'true' MUST appear no more than once.")
        };

        private static IEnumerable<SchemaAttributeResponse> UserRoleAttributes = new SchemaAttributeResponse[]
        {
            SchemaAttribute.CreateValueAttribute("The value of a role."),
            SchemaAttribute.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY."),
            SchemaAttribute.CreateTypeAttribute("A label indicating the attribute's function.", new string[] { }),
            SchemaAttribute.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute.  The primary attribute value 'true' MUST appear no more than once.")
        };

        private static IEnumerable<SchemaAttributeResponse> UserCertificateAttributes = new SchemaAttributeResponse[]
        {
            SchemaAttribute.CreateValueAttribute("The value of an X.509 certificate.", type: Constants.SchemaAttributeTypes.Binary),
            SchemaAttribute.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY."),
            SchemaAttribute.CreateTypeAttribute("A label indicating the attribute's function.", new string[] { }),
            SchemaAttribute.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute.  The primary attribute value 'true' MUST appear no more than once.")
        };

        private static IEnumerable<SchemaAttributeResponse> UserNameAttributeSub = new SchemaAttributeResponse[]
        {
             SchemaAttribute.CreateAttribute(Constants.NameResponseNames.Formatted, "The full name, including all middle names, titles, and suffixes as appropriate, formatted for display (e.g., 'Ms. Barbara J Jensen, III')."),
             SchemaAttribute.CreateAttribute(Constants.NameResponseNames.FamilyName, "The family name of the User, or last name in most Western languages (e.g., 'Jensen' given the fullname 'Ms. Barbara J Jensen, III')."),
             SchemaAttribute.CreateAttribute(Constants.NameResponseNames.GivenName, "The given name of the User, or first name in most Western languages (e.g., 'Barbara' given the full name 'Ms. Barbara J Jensen, III')."),
             SchemaAttribute.CreateAttribute(Constants.NameResponseNames.MiddleName, "The middle name(s) of the User (e.g., 'Jane' given the full name 'Ms. Barbara J Jensen, III')."),
             SchemaAttribute.CreateAttribute(Constants.NameResponseNames.HonorificPrefix, "The honorific prefix(es) of the User, or title in most Western languages (e.g., 'Ms.' given the full name 'Ms. Barbara J Jensen, III')."),
             SchemaAttribute.CreateAttribute(Constants.NameResponseNames.HonorificPrefix, "The honorific suffix(es) of the User, or suffix in most Western languages (e.g., 'III' given the full name 'Ms. Barbara J Jensen, III').")
        };

        private static IEnumerable<SchemaAttributeResponse> UserAddressAttributes = new SchemaAttributeResponse[]
        {
             SchemaAttribute.CreateAttribute(Constants.AddressResponseNames.Formatted, "The full mailing address, formatted for display or use with a mailing label.  This attribute MAY contain newlines."),
             SchemaAttribute.CreateAttribute(Constants.AddressResponseNames.StreetAddress, "The full street address component, which may include house number, street name, P.O. box, and multi-line extended street address information.  This attribute MAY contain newlines."),
             SchemaAttribute.CreateAttribute(Constants.AddressResponseNames.Locality, "The city or locality component."),
             SchemaAttribute.CreateAttribute(Constants.AddressResponseNames.Region, "The state or region component."),
             SchemaAttribute.CreateAttribute(Constants.AddressResponseNames.PostalCode, "The zip code or postal code component."),
             SchemaAttribute.CreateAttribute(Constants.AddressResponseNames.Country, "The country name component."),
             SchemaAttribute.CreateTypeAttribute("A label indicating the attribute's function, e.g., 'work' or 'home'.", new string[] { "work", "home", "other" })
        };

        #endregion

        private static SchemaResponse UserSchema = new SchemaResponse
        {
            Id = Constants.SchemaUrns.User,
            Name = Constants.ResourceTypes.User,
            Description = "User Account",
            Attributes = CommonAttributes.Concat(new SchemaAttributeResponse[]
            {
                // user name
                SchemaAttribute.CreateAttribute(
                    Constants.UserResourceResponseNames.UserName,
                    "Unique identifier for the User, typically"+
                                    "used by the user to directly authenticate to the service provider."+
                                    "Each User MUST include a non-empty userName value.  This identifier"+
                                    "MUST be unique across the service provider's entire set of Users."+
                                    "REQUIRED.",
                    uniqueness: Constants.SchemaAttributeUniqueness.Server,
                    required : true),
                // name
                SchemaAttribute.CreateComplexAttribute(
                    Constants.UserResourceResponseNames.Name,
                     "The components of the user's real name."+
                                    "Providers MAY return just the full name as a single string in the"+
                                    "formatted sub-attribute, or they MAY return just the individual"+
                                    "component attributes using the other sub-attributes, or they MAY"+
                                    "return both.If both variants are returned, they SHOULD be"+
                                    "describing the same name, with the formatted name indicating how the"+
                                    "component attributes should be combined.",
                     UserNameAttributeSub),
                // Display name
                SchemaAttribute.CreateAttribute(
                     Constants.UserResourceResponseNames.DisplayName,
                     "The name of the User, suitable for display"+
                                    "to end-users.  The name SHOULD be the full name of the User being"+
                                    "described, if known."),
                // Nick name
                SchemaAttribute.CreateAttribute(
                     Constants.UserResourceResponseNames.NickName,
                     "The casual way to address the user in real"+
                                    "life, e.g., 'Bob' or 'Bobby' instead of 'Robert'.  This attribute"+
                                    "SHOULD NOT be used to represent a User's username (e.g., 'bjensen' or"+
                                    "'mpepperidge')."),
                // Profile url
                SchemaAttribute.CreateAttribute(
                     Constants.UserResourceResponseNames.ProfileUrl,
                     "A fully qualified URL pointing to a page"+
                                    "representing the User's online profile.",
                     type: Constants.SchemaAttributeTypes.Reference,
                     referenceTypes: new string [] { "external" }),
                // Title
                SchemaAttribute.CreateAttribute(
                     Constants.UserResourceResponseNames.Title,
                     "The user's title, such as"+
                                    "\"Vice President.\""),
                // User type
                SchemaAttribute.CreateAttribute(
                     Constants.UserResourceResponseNames.UserType,
                     "Used to identify the relationship between"+
                                    "the organization and the user.  Typical values used might be"+
                                    "'Contractor', 'Employee', 'Intern', 'Temp', 'External', and"+
                                    "'Unknown', but any value may be used."),
                // preferred language
                SchemaAttribute.CreateAttribute(
                     Constants.UserResourceResponseNames.PreferredLanguage,
                     "Indicates the User's preferred written or"+
                                    "spoken language.  Generally used for selecting a localized user"+
                                    "interface; e.g., 'en_US' specifies the language English and country"+
                                    "US."),
                // locale
                SchemaAttribute.CreateAttribute(
                     Constants.UserResourceResponseNames.Locale,
                     "Used to indicate the User's default location"+
                                    "for purposes of localizing items such as currency, date time format, or"+
                                    "numerical representations."),
                // time zone
                SchemaAttribute.CreateAttribute(
                     Constants.UserResourceResponseNames.Timezone,
                     "The User's time zone in the 'Olson' time zone"+
                                "database format, e.g., 'America/Los_Angeles'."),
                // active
                SchemaAttribute.CreateAttribute(
                     Constants.UserResourceResponseNames.Active,
                     "A Boolean value indicating the User's"+
                                    "administrative status.",
                     uniqueness: string.Empty,
                     caseExact : false, 
                     type: Constants.SchemaAttributeTypes.Boolean),
                // password
                SchemaAttribute.CreateAttribute(
                     Constants.UserResourceResponseNames.Password,
                     "The User's cleartext password.  This"+
                                    "attribute is intended to be used as a means to specify an initial"+
                                    "password when creating a new User or to reset an existing User's"+
                                    "password.",
                     returned: Constants.SchemaAttributeReturned.Never,
                     mutability: Constants.SchemaAttributeMutability.writeOnly),
                // Emails
                SchemaAttribute.CreateComplexAttribute(
                     Constants.UserResourceResponseNames.Emails,
                     "Email addresses for the user.  The value"+
                        "SHOULD be canonicalized by the service provider, e.g.,"+
                        "'bjensen@example.com' instead of 'bjensen@EXAMPLE.COM'."+
                        "Canonical type values of 'work', 'home', and 'other'.",
                     EmailAttributeSub,
                     multiValued: true),
                // Phone numbers
                SchemaAttribute.CreateComplexAttribute(
                    Constants.UserResourceResponseNames.Phones,
                    "Phone numbers for the User.  The value"+
                        "SHOULD be canonicalized by the service provider according to the"+
                        "format specified in RFC 3966, e.g., 'tel:+1-201-555-0123'."+
                        "Canonical type values of 'work', 'home', 'mobile', 'fax', 'pager',"+
                        "and 'other'.",
                    UserPhoneNumberAttributes,
                    multiValued: true),
                // Ims
                SchemaAttribute.CreateComplexAttribute(
                    Constants.UserResourceResponseNames.Ims,
                    "Instant messaging addresses for the User.",
                    UserImsAttributes,
                    multiValued: true),
                // Addresses
                SchemaAttribute.CreateComplexAttribute(
                    Constants.UserResourceResponseNames.Addresses,
                    "A physical mailing address for this User. Canonical type values of 'work', 'home', and 'other'.  This attribute is a complex type with the following sub-attributes.",
                    UserAddressAttributes,
                    multiValued: true),
                // Groups
                SchemaAttribute.CreateComplexAttribute(
                    Constants.UserResourceResponseNames.Groups,
                    "A list of groups to which the user belongs, either through direct membership, through nested groups, or dynamically calculated.",
                    UserGroupAttributes,
                    multiValued: true,
                    mutability: Constants.SchemaAttributeMutability.ReadOnly),
                // Entitlements
                SchemaAttribute.CreateComplexAttribute(
                    Constants.UserResourceResponseNames.Entitlements,
                    "A list of entitlements for the User that represent a thing the User has.",
                    UserEntitlementAttributes,
                    multiValued: true),
                // Roles
                SchemaAttribute.CreateComplexAttribute(
                    Constants.UserResourceResponseNames.Roles,
                    "A list of roles for the User that collectively represent who the User is, e.g., 'Student', 'Faculty'.",
                    UserRoleAttributes,
                    multiValued: true),
                // Certificate
                SchemaAttribute.CreateComplexAttribute(
                    Constants.UserResourceResponseNames.X509Certificates,
                    "A list of certificates issued to the User.",
                    UserCertificateAttributes,
                    multiValued: true),
            }),
            Meta = new MetaResponse
            {
                ResourceType = "Schema",
                Location = Constants.SchemaUrns.User
            }
        };

        #endregion

        #region Group

        private static IEnumerable<SchemaAttributeResponse> GroupMembersAttribute = new SchemaAttributeResponse[]
        {
            SchemaAttribute.CreateAttribute(Constants.GroupMembersResponseNames.Value, "Identifier of the member of this Group.", uniqueness: Constants.SchemaAttributeUniqueness.None, required : false, mutability: Constants.SchemaAttributeMutability.Immutable),
            SchemaAttribute.CreateRefAttribute("The URI corresponding to a SCIM resource that is a member of this Group.", new string[] { "User", "Group" }, Constants.SchemaAttributeMutability.Immutable),
            SchemaAttribute.CreateTypeAttribute("A label indicating the type of resource, e.g., 'User' or 'Group'.", new string[] { "User", "Group" }, Constants.SchemaAttributeMutability.Immutable)
        };

        private static SchemaResponse GroupSchema = new SchemaResponse
        {
            Id = Constants.SchemaUrns.Group,
            Name = Constants.ResourceTypes.Group,
            Description = "Group",
            Attributes = CommonAttributes.Concat(new SchemaAttributeResponse[]
            {
                // display name
                SchemaAttribute.CreateAttribute(Constants.GroupResourceResponseNames.DisplayName, "A human-readable name for the Group. REQUIRED.", uniqueness: Constants.SchemaAttributeUniqueness.None, required : false),
                // members
                SchemaAttribute.CreateComplexAttribute(Constants.GroupResourceResponseNames.Members, "A list of members of the Group.", GroupMembersAttribute, multiValued: true),
            }),
            Meta = new MetaResponse
            {
                ResourceType = "Schema",
                Location = Constants.SchemaUrns.Group
            }
        };

        #endregion

        private static IEnumerable<SchemaResponse> _schemas = new SchemaResponse[]
        {
            UserSchema,
            GroupSchema
        };

        #region Public methods

        public IEnumerable<SchemaResponse> GetSchemas()
        {
            return _schemas;
        }

        public SchemaResponse GetSchema(string id)
        {
            return _schemas.FirstOrDefault(s => s.Id == id);
        }

        public IEnumerable<SchemaAttributeResponse> GetCommonAttributes()
        {
            return CommonAttributes;
        }

        #endregion
    }
}
