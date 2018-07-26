using Newtonsoft.Json;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Uma.Client.Results;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Client.ResourceSet
{
    public interface ISearchResourcesOperation
    {
        Task<SearchResourceSetResult> ExecuteAsync(string url, SearchResourceSet parameter, string authorizationHeaderValue = null);
    }

    internal sealed class SearchResourcesOperation : ISearchResourcesOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SearchResourcesOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SearchResourceSetResult> ExecuteAsync(string url, SearchResourceSet parameter, string authorizationHeaderValue = null)
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

            var httpResult = await httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new SearchResourceSetResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new SearchResourceSetResult
            {
                Content = JsonConvert.DeserializeObject<SearchResourceSetResponse>(content)
            };
        }
    }
}
