
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

namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    public interface IAddResourceOwnerOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, AddResourceOwnerRequest resourceOwner, string authorizationHeaderValue = null);
    }

    internal sealed class AddResourceOwnerOperation : IAddResourceOwnerOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AddResourceOwnerOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, AddResourceOwnerRequest resourceOwner, string authorizationHeaderValue = null)
        {
            if (resourceOwnerUri == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerUri));
            }

            if (resourceOwner == null)
            {
                throw new ArgumentNullException(nameof(resourceOwner));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedJson = JObject.FromObject(resourceOwner).ToString();
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = resourceOwnerUri,
                Content = body
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
