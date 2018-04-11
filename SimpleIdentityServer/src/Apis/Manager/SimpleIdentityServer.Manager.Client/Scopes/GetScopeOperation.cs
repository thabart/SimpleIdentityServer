using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Scopes
{
    public interface IGetScopeOperation
    {
        Task<GetScopeResponse> ExecuteAsync(Uri scopesUri, string authorizationHeaderValue = null);
    }

    internal sealed class GetScopeOperation : IGetScopeOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetScopeOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetScopeResponse> ExecuteAsync(Uri scopesUri, string authorizationHeaderValue = null)
        {
            if (scopesUri == null)
            {
                throw new ArgumentNullException(nameof(scopesUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = scopesUri
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var httpResult = await httpClient.SendAsync(request);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var rec = JObject.Parse(content);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                var err = JsonConvert.DeserializeObject<ErrorResponse>(content);
                return new GetScopeResponse(err);
            }
            catch (Exception)
            {
                return new GetScopeResponse
                {
                    ContainsError = true
                };
            }

            var scope = JsonConvert.DeserializeObject<ScopeResponse>(content);
            return new GetScopeResponse
            {
                Content = scope
            };
        }
    }
}
