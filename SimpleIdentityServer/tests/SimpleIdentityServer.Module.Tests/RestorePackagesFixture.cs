using Newtonsoft.Json;
using SimpleIdentityServer.Module.Loader;
using SimpleIdentityServer.Module.Tests.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Module.Tests
{
    public class RestorePackagesFixture
    {
        private static IEnumerable<string> _nugetSources = new List<string>
        {
            "https://api.nuget.org/v3/index.json",
            "https://www.myget.org/F/advance-ict/api/v3/index.json"
        };

        [Fact]
        public async Task WhenRestorePackageThenPackagesAreInstalled()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var configurationFile = Path.Combine(currentDirectory, "config.json");
            var configuration = JsonConvert.DeserializeObject<ProjectConfiguration>(File.ReadAllText(configurationFile));
            foreach (var module in configuration.Modules)
            {
                foreach(var package in module.Packages)
                {
                    await RestoreNugetPackage(package.Library, package.Version);
                    /*
                    var url = "https://www.myget.org/F/advance-ict/api/v3/flatcontainer/SimpleIdentityServer.EF/3.0.0-rc6/"+ package.Library + ".nupkg";
                    var httpClient = new HttpClient();
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        using (var contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync())
                        {
                            using (var stream = new FileStream(package.Library + ".nupkg", FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await contentStream.CopyToAsync(stream);
                            }
                        }
                    }

                    ZipFile.ExtractToDirectory(package.Library + ".nupkg", package.Library);
                    File.Delete(package.Library + ".nupkg");
                    */
                }
            }

            // 1. Download the nuget package.
            
            
            // 2. Extract the nuget package into folder.
            ZipFile.ExtractToDirectory("SimpleIdentityServer.EF.nupkg", "SimpleIdentityServer.EF");
            string s = "";
        }

        private async Task RestoreNugetPackage(string packageName, string version)
        {
            var httpClient = new HttpClient();
            foreach(var nuget in _nugetSources)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, nuget);
                    var httpResponse = await httpClient.SendAsync(request).ConfigureAwait(false);
                    var json = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var configurationResponse = JsonConvert.DeserializeObject<NugetConfigurationResponse>(json);
                    var resource = configurationResponse.Resources.FirstOrDefault(r => r.Type.Contains("PackageBaseAddress"));
                    request = new HttpRequestMessage(HttpMethod.Get, resource.Id + packageName + "/index.json");
                    httpResponse = await httpClient.SendAsync(request).ConfigureAwait(false);
                    httpResponse.EnsureSuccessStatusCode();
                }
                catch(Exception)
                {

                }
            }
        }
    }
}
