using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    public interface IGetResourceOwnerOperation
    {
        Task<GetResourceOwnerResponse> ExecuteAsync(Uri resourceOwnerUri, string authorizationHeaderValue = null);
    }

    internal sealed class GetResourceOwnerOperation : IGetResourceOwnerOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetResourceOwnerOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetResourceOwnerResponse> ExecuteAsync(Uri clientsUri, string authorizationHeaderValue = null)
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
            catch (HttpRequestException)
            {
                var err = JsonConvert.DeserializeObject<ErrorResponse>(content);
                return new GetResourceOwnerResponse(err);
            }
            catch (Exception)
            {
                return new GetResourceOwnerResponse
                {
                    ContainsError = true
                };
            }

            var resourceOwner = JsonConvert.DeserializeObject<ResourceOwnerResponse>(content);
            return new GetResourceOwnerResponse
            {
                Content = resourceOwner
            };
        }
    }
}
