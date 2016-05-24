namespace SimpleIdentityServer.DataAccess.SqlServer.Models
{
    public class Address
    {
        public int Id { get; set; }

        public string ResourceOwnerForeignKey { get; set; }

        public string Formatted { get; set; }

        public string StreetAddress { get; set; }

        public string Locality { get; set; }

        public string Region { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public ResourceOwner ResourceOwner { get; set; }
    }
}
