using Newtonsoft.Json;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Scim.Mapping.Ad.Client.Results;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Mapping
{
    public interface IGetAdMappingOperation
    {
        Task<GetAdMappingResult> Execute(string attributeId, string url, string accessToken = null);
    }

    internal sealed class GetAdMappingOperation : IGetAdMappingOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetAdMappingOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetAdMappingResult> Execute(string attributeId, string url, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(attributeId))
            {
                throw new ArgumentNullException(nameof(attributeId));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{url}/mappings/{attributeId}")
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
                return new GetAdMappingResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content)
                };
            }

            return new GetAdMappingResult
            {
                Content = JsonConvert.DeserializeObject<MappingResponse>(content)
            };
        }
    }
}
