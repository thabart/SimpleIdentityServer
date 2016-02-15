namespace SimpleIdentityServer.Api.Tests.Common.Fakes.Models
{
    public enum FakeScopeType
    {
        ProtectedApi,
        ResourceOwner
    }

    public class FakeScope
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsInternal { get; set; }

        public bool IsExposed { get; set; }

        public bool IsDisplayedInConsent { get; set; }

        public FakeScopeType Type { get; set; }

        public string Claims { get; set; }
    }
}
