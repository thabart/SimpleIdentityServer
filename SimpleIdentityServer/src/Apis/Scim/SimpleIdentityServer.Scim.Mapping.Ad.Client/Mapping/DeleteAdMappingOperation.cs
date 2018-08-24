using Newtonsoft.Json;
using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Mapping
{
    public interface IDeleteAdMappingOperation
    {
        Task<BaseResponse> Execute(string attributeId, string url, string accessToken = null);
    }

    internal sealed class DeleteAdMappingOperation : IDeleteAdMappingOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteAdMappingOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> Execute(string attributeId, string url, string accessToken = null)
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
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{url}/{attributeId}")
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
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content)
                };
            }

            return new BaseResponse();
        }
    }
}
