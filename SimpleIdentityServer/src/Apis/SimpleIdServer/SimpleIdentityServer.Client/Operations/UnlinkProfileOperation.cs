using Newtonsoft.Json;
using SimpleIdentityServer.Client.Results;
using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Operations
{
    public interface IUnlinkProfileOperation
    {
        Task<BaseResponse> Execute(string requestUrl, string externalSubject, string currentSubject, string authorizationHeaderValue = null);
        Task<BaseResponse> Execute(string requestUrl, string externalSubject, string authorizationHeaderValue = null);
    }

    internal sealed class UnlinkProfileOperation : IUnlinkProfileOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UnlinkProfileOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task<BaseResponse> Execute(string requestUrl, string externalSubject, string currentSubject, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentException(nameof(requestUrl));
            }

            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }
         
            if (string.IsNullOrWhiteSpace(currentSubject))
            {
                throw new ArgumentNullException(nameof(currentSubject));
            }

            var url = requestUrl + $"/{currentSubject}/{externalSubject}";
            return Delete(url, authorizationHeaderValue);
        }

        public Task<BaseResponse> Execute(string requestUrl, string externalSubject, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentException(nameof(requestUrl));
            }

            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }
            
            var url = requestUrl + $"/.me/{externalSubject}";
            return Delete(url, authorizationHeaderValue);
        }

        private async Task<BaseResponse> Delete(string url, string authorizationValue = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };
            if (!string.IsNullOrWhiteSpace(authorizationValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationValue);
            }

            var httpClient = _httpClientFactory.GetHttpClient();
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

            return new BaseResponse();
        }
    }
}
