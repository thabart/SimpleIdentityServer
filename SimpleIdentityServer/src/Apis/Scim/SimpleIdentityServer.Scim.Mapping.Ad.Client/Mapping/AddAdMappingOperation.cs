using Newtonsoft.Json;
using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Scim.Mapping.Ad.Common.DTOs.Requests;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Client.Mapping
{
    public interface IAddAdMappingOperation
    {
        Task<BaseResponse> Execute(AddMappingRequest addMappingRequest, string url, string accessToken = null);
    }

    internal sealed class AddAdMappingOperation : IAddAdMappingOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AddAdMappingOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> Execute(AddMappingRequest addMappingRequest, string url, string accessToken = null)
        {
            if(addMappingRequest == null)
            {
                throw new ArgumentNullException(nameof(addMappingRequest));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }
            
            var httpClient = _httpClientFactory.GetHttpClient();
            var json = JsonConvert.SerializeObject(addMappingRequest, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json),
                RequestUri = new Uri(url)
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
            catch(Exception)
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
