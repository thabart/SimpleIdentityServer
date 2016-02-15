namespace SimpleIdentityServer.Api.Tests.Common.Fakes.Models
{
    public class FakeJwsPayload
    {
        public string iss { get; set; }

        public string sub { get; set; }

        public string jti { get; set; }

        public double exp { get; set; }
    }
}
