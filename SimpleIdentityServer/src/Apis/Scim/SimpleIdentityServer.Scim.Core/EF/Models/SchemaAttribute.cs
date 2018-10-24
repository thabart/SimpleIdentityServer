using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Core.EF.Models
{
    public class SchemaAttribute
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool MultiValued { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public string CanonicalValues { get; set; }
        public bool CaseExact { get; set; }
        public string Mutability { get; set; }
        public string Returned { get; set; }
        public string Uniqueness { get; set; }
        public string ReferenceTypes { get; set; }
        public string SchemaAttributeIdParent { get; set; }
        public string SchemaId { get; set; }
        public bool IsCommon { get; set; }
        public virtual SchemaAttribute Parent { get; set; }
        public virtual List<RepresentationAttribute> RepresentationAttributes { get; set; }
        public virtual List<SchemaAttribute> Children { get; set; }
        public virtual Schema Schema { get; set; }
    }
}
