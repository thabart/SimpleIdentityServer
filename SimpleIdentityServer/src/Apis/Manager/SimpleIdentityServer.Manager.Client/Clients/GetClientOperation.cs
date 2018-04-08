using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Clients
{
    public interface IGetClientOperation
    {
        Task<GetClientResponse> ExecuteAsync(Uri clientsUri, string authorizationHeaderValue = null);
    }

    internal sealed class GetClientOperation : IGetClientOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetClientOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetClientResponse> ExecuteAsync(Uri clientsUri, string authorizationHeaderValue = null)
        {
            if (clientsUri == null)
            {
                throw new ArgumentNullException(nameof(clientsUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = clientsUri
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
            catch(HttpRequestException)
            {
                var err = JsonConvert.DeserializeObject<ErrorResponse>(content);
                return new GetClientResponse(err);
            }
            catch(Exception)
            {
                return new GetClientResponse
                {
                    ContainsError = true
                };
            }

            var client = JsonConvert.DeserializeObject<ClientResponse>(content);
            return new GetClientResponse
            {
                Content = client
            };
        }
    }
}
