using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Scopes
{
    public interface ISearchScopesOperation
    {
        Task<SearchScopeResponse> ExecuteAsync(Uri scopesUri, SearchScopesRequest parameter, string authorizationHeaderValue = null);
    }

    internal sealed class SearchScopesOperation : ISearchScopesOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SearchScopesOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SearchScopeResponse> ExecuteAsync(Uri clientsUri, SearchScopesRequest parameter, string authorizationHeaderValue = null)
        {
            if (clientsUri == null)
            {
                throw new ArgumentNullException(nameof(clientsUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedPostPermission = JsonConvert.SerializeObject(parameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = clientsUri,
                Content = body
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
                var resp = JsonConvert.DeserializeObject<ErrorResponse>(content);
                return new SearchScopeResponse(resp);
            }
            catch (Exception)
            {
                return new SearchScopeResponse
                {
                    ContainsError = true
                };
            }

            return new SearchScopeResponse
            {
                Content = JsonConvert.DeserializeObject<SearchScopesResponse>(content)
            };
        }
    }
}
