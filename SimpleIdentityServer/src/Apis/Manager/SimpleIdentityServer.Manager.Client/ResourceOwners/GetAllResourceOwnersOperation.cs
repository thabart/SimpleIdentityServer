using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    public interface IGetAllResourceOwnersOperation
    {
        Task<GetAllResourceOwnersResponse> ExecuteAsync(Uri resourceOwnerUri, string authorizationHeaderValue = null);
    }

    internal sealed class GetAllResourceOwnersOperation : IGetAllResourceOwnersOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetAllResourceOwnersOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetAllResourceOwnersResponse> ExecuteAsync(Uri resourceOwnerUri, string authorizationHeaderValue = null)
        {
            if (resourceOwnerUri == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
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
                var rec = JsonConvert.DeserializeObject<ErrorResponse>(content);
                return new GetAllResourceOwnersResponse(rec);
            }
            catch (Exception)
            {
                return new GetAllResourceOwnersResponse
                {
                    ContainsError = true
                };
            }

            var jArr = JArray.Parse(content);
            var result = new List<ResourceOwnerResponse>();
            foreach (JObject rec in jArr)
            {
                var client = JsonConvert.DeserializeObject<ResourceOwnerResponse>(rec.ToString());
                if (client != null)
                {
                    result.Add(client);
                }
            }

            return new GetAllResourceOwnersResponse
            {
                Content = result
            };
        }
    }
}
