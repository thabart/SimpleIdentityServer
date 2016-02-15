using System;
using SimpleIdentityServerClient;
using SimpleIdentityServerClient.Parameters;

namespace SimpleIdentityServer.Client.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new IdentityServerClientFactory();
            var client = factory.CreateClient("http://localhost:50470/swagger/docs/v1", "http://localhost:50470");
            var request = new GetAccessToken
            {
                ClientId = "MyBlog",
                Username = "administrator",
                Password = "password",
                Scope = "BlogApi"
            };
            var result = client.GetAccessTokenViaResourceOwnerGrantTypeAsync(request).Result;

            Console.WriteLine(result.AccessToken);

            Console.ReadLine();
        }
    }
}
