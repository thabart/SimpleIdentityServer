using Newtonsoft.Json;
using SimpleIdentityServer.Module.Loader.Factories;
using SimpleIdentityServer.Module.Loader.Nuget.DTOs.Responses;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Module.Loader.Nuget
{
    public interface INugetClient
    {
        Task<NugetConfigurationResponse> GetConfiguration(string url);
        Task<NugetFlatContainerResponse> GetNugetFlatContainer(string url, string packageName);
        Task<Stream> DownloadNugetPackage(string url, string packageName, string version);
    }

    internal sealed class NugetClient : INugetClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        public NugetClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<NugetConfigurationResponse> GetConfiguration(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var httpResponse = await httpClient.SendAsync(request).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();
            var json = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<NugetConfigurationResponse>(json);
        }

        public async Task<NugetFlatContainerResponse> GetNugetFlatContainer(string url, string packageName)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(packageName))
            {
                throw new ArgumentNullException(nameof(packageName));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url + packageName + "/index.json");
            var httpResponse = await httpClient.SendAsync(request).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();
            var json = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<NugetFlatContainerResponse>(json);
        }

        public async Task<Stream> DownloadNugetPackage(string url, string packageName, string version)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(packageName))
            {
                throw new ArgumentNullException(nameof(packageName));
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentNullException(nameof(version));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{url}{packageName}/{version}/{packageName}.{version}.nupkg");
            return await (await httpClient.SendAsync(request).ConfigureAwait(false)).Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
    }
}