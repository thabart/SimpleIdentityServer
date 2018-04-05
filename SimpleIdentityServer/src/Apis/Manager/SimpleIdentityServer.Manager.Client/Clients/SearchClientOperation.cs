using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Client.DTOs.Parameters;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Clients
{
    public interface ISearchClientOperation
    {
        Task<SearchClientResponse> ExecuteAsync(Uri clientsUri, SearchClientParameter parameter, string authorizationHeaderValue = null);
    }

    internal sealed class SearchClientOperation : ISearchClientOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SearchClientOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SearchClientResponse> ExecuteAsync(Uri clientsUri, SearchClientParameter parameter, string authorizationHeaderValue = null)
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
                ErrorResponse resp = null;
                if (rec != null)
                {
                    ErrorResponse.ToError(rec);
                }

                return new SearchClientResponse(resp);
            }
            catch (Exception)
            {
                return new SearchClientResponse
                {
                    ContainsError = true
                };
            }

            return SearchClientResponse.ToSearchClientResponse(rec);
        }
    }
}
