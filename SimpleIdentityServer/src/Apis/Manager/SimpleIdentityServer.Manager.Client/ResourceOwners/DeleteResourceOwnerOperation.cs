using Newtonsoft.Json;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    public interface IDeleteResourceOwnerOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, string authorizationHeaderValue = null);
    }

    internal sealed class DeleteResourceOwnerOperation : IDeleteResourceOwnerOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteResourceOwnerOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, string authorizationHeaderValue = null)
        {
            if (resourceOwnerUri == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = resourceOwnerUri
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var httpResult = await httpClient.SendAsync(request);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                var resp = JsonConvert.DeserializeObject<ErrorResponse>(content);
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = resp
                };
            }
            catch (Exception)
            {
                return new BaseResponse
                {
                    ContainsError = true
                };
            }

            return new BaseResponse();
        }
    }
}
