using Newtonsoft.Json;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Scim.Mapping.Ad.Client.Results;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Configuration
{
    public interface IGetAdConfigurationOperation
    {
        Task<GetAdConfigurationResult> Execute(string baseUrl, string accessToken = null);
    }

    internal sealed class GetAdConfigurationOperation : IGetAdConfigurationOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetAdConfigurationOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetAdConfigurationResult> Execute(string baseUrl, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{baseUrl}/adconfiguration")
            };
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            var result = await httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetAdConfigurationResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content)
                };
            }

            return new GetAdConfigurationResult
            {
                Content = JsonConvert.DeserializeObject<AdConfigurationResponse>(content)
            };
        }
    }
}
