using Newtonsoft.Json;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    public interface IUpdateResourceOwnerOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, ResourceOwnerResponse resourceOwner, string authorizationHeaderValue = null);
    }

    internal sealed class UpdateResourceOwnerOperation : IUpdateResourceOwnerOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UpdateResourceOwnerOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> ExecuteAsync(Uri resourceOwnerUri, ResourceOwnerResponse resourceOwner, string authorizationHeaderValue = null)
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
            var serializedJson = JsonConvert.SerializeObject(resourceOwner).ToString();
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
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
                var resp = JsonConvert.DeserializeObject<ErrorResponse>(content);
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = resp
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
