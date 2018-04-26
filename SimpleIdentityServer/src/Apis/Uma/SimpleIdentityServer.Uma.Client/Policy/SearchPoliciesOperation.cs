using Newtonsoft.Json;
using SimpleIdentityServer.Uma.Client.Factory;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Client.Policy
{
    public interface ISearchPoliciesOperation
    {
        Task<SearchAuthPoliciesResponse> ExecuteAsync(string url, SearchAuthPolicies parameter, string authorizationHeaderValue = null);
    }

    internal sealed class SearchPoliciesOperation : ISearchPoliciesOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SearchPoliciesOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SearchAuthPoliciesResponse> ExecuteAsync(string url, SearchAuthPolicies parameter, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedPostPermission = JsonConvert.SerializeObject(parameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = body
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var httpResult = await httpClient.SendAsync(request);
            httpResult.EnsureSuccessStatusCode();
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<SearchAuthPoliciesResponse>(content);
        }
    }
}
