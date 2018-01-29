#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace SimpleIdentityServer.Startup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // To launch the application : dotnet run --server.urls=http://*:5000
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                .Build();
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    var httpsOptions = new HttpsConnectionFilterOptions
                    {
                        ServerCertificate = new X509Certificate2("SimpleIdServer.pfx")
                    };
                    options.UseHttps(httpsOptions);
                })
                .UseUrls(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                // .UseConfiguration(configuration)
                .UseStartup<Startup>()
                .Build();
            host.Run();
        }
    }
}
