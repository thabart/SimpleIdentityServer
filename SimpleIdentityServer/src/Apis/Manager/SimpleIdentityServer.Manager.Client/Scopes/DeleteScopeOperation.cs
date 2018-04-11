using Newtonsoft.Json;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Scopes
{
    public interface IDeleteScopeOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri scopesUri, string authorizationHeaderValue = null);
    }

    internal sealed class DeleteScopeOperation : IDeleteScopeOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteScopeOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> ExecuteAsync(Uri scopesUri, string authorizationHeaderValue = null)
        {
            if (scopesUri == null)
            {
                throw new ArgumentNullException(nameof(scopesUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = scopesUri
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
                var rec = JsonConvert.DeserializeObject<ErrorResponse>(content);
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = rec
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
