namespace SimpleIdentityServer.AccountFilter.Basic.EF.Models
{
    public class FilterRule
    {
        public string Id { get; set; }
        public string FilterId { get; set; }
        public string ClaimKey { get; set; }
        public string ClaimValue { get; set; }
        public int Operation { get; set; }
        public virtual Filter Filter { get; set; }
    }
}