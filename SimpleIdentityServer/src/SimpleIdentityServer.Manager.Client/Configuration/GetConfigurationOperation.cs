using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Configuration
{
    public interface IGetConfigurationOperation
    {
        Task<ConfigurationResponse> ExecuteAsync(Uri wellKnownConfigurationUrl);
    }

    internal sealed class GetConfigurationOperation : IGetConfigurationOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetConfigurationOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ConfigurationResponse> ExecuteAsync(Uri wellKnownConfigurationUri)
        {
            if (wellKnownConfigurationUri == null)
            {
                throw new ArgumentNullException(nameof(wellKnownConfigurationUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = wellKnownConfigurationUri
            };
            var httpResult = await httpClient.SendAsync(request).ConfigureAwait(false);
            httpResult.EnsureSuccessStatusCode();
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var jObj = JObject.Parse(content);
            return new ConfigurationResponse
            {
                ClientsEndpoint = jObj.GetValue(Constants.ConfigurationResponseNames.ClientsEndpoint).ToString(),
                JweEndpoint = jObj.GetValue(Constants.ConfigurationResponseNames.JweEndpoint).ToString(),
                JwsEndpoint = jObj.GetValue(Constants.ConfigurationResponseNames.JwsEndpoint).ToString(),
                ManageEndpoint = jObj.GetValue(Constants.ConfigurationResponseNames.ManageEndpoint).ToString(),
                ResourceOwnersEndpoint = jObj.GetValue(Constants.ConfigurationResponseNames.ResourceOwnersEndpoint).ToString(),
                ScopesEndpoint = jObj.GetValue(Constants.ConfigurationResponseNames.ScopesEndpoint).ToString(),
            };
        }
    }
}
