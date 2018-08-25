using Newtonsoft.Json;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Scim.Mapping.Ad.Client.Results;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Mapping
{
    public interface IGetAdPropertiesOperation
    {
        Task<GetAdPropertiesResult> Execute(string url, string accessToken = null);
    }

    internal sealed class GetAdPropertiesOperation : IGetAdPropertiesOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetAdPropertiesOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetAdPropertiesResult> Execute(string url, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{url}/mappings/properties")
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
                return new GetAdPropertiesResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content)
                };
            }

            return new GetAdPropertiesResult
            {
                Content = JsonConvert.DeserializeObject<IEnumerable<string>>(content)
            };
        }
    }
}
