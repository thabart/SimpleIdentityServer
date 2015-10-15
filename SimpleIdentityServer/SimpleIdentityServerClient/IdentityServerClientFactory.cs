namespace SimpleIdentityServerClient
{
    public class IdentityServerClientFactory
    {
        public IIdentityServerClient CreateClient(string url)
        {
            // THE URL should be : http://localhost:50470/swagger/docs/v1
            return new IdentityServerClient();
        }
    }
}
