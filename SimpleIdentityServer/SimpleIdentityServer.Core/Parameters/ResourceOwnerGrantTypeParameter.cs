namespace SimpleIdentityServer.Core.Parameters
{
    public sealed class ResourceOwnerGrantTypeParameter
    {
        public string ClientId { get; set; }

        public string Scope { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
