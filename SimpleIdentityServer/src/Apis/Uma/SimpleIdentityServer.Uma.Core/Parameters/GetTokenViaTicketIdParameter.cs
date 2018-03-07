namespace SimpleIdentityServer.Uma.Core.Parameters
{
    public class GetTokenViaTicketIdParameter
    {
        public string Ticket { get; set; }
        public string ClaimToken { get; set; }
        public string ClaimTokenFormat { get; set; }
        public string Pct { get; set; }
        public string Rpt { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientAssertionType { get; set; }
        public string ClientAssertion { get; set; }
    }
}
