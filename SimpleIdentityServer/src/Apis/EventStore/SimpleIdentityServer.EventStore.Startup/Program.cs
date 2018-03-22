using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace SimpleIdentityServer.EventStore.Startup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                .Build();
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(args)
                .UseStartup<Startup>()
                .Build();
            host.Run();
        }
    }
}
