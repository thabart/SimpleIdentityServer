using Newtonsoft.Json;
using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccountFilter.Basic.Client.Operations
{
    public interface IDeleteFilterOperation
    {
        Task<BaseResponse> Execute(string requestUrl, string filterId, string authorizationHeaderValue = null);
    }

    internal sealed class DeleteFilterOperation : IDeleteFilterOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteFilterOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> Execute(string requestUrl, string filterId, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            if (string.IsNullOrWhiteSpace(filterId))
            {
                throw new ArgumentNullException(nameof(filterId));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(requestUrl + $"/{filterId}")
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
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
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new BaseResponse
            {
                HttpStatus = result.StatusCode
            };
        }
    }
}
