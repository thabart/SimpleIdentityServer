using Newtonsoft.Json;
using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Requests;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Configuration
{
    public interface IUpdateAdConfigurationOperation
    {
        Task<BaseResponse> Execute(UpdateAdConfigurationRequest request, string baseUrl, string accessToken = null);
    }

    internal sealed class UpdateAdConfigurationOperation : IUpdateAdConfigurationOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UpdateAdConfigurationOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> Execute(UpdateAdConfigurationRequest updateAdConfigurationRequest, string baseUrl, string accessToken = null)
        {
            if (updateAdConfigurationRequest == null)
            {
                throw new ArgumentNullException(nameof(updateAdConfigurationRequest));
            }

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var json = JsonConvert.SerializeObject(updateAdConfigurationRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                Content = new StringContent(json),
                RequestUri = new Uri($"{baseUrl}/adconfiguration")
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
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
