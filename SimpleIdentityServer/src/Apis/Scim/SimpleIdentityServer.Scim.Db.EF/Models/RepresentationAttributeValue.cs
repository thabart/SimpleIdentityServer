namespace SimpleIdentityServer.Scim.Db.EF.Models
{
    public class RepresentationAttributeValue
    {
        public string Id { get; set; }
        public string RepresentationAttributeId { get; set; }
        public string Value { get; set; }
        public virtual RepresentationAttribute RepresentationAttribute { get; set; }
    }
}
