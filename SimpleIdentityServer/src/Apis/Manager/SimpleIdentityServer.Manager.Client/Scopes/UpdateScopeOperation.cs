using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Scopes
{
    public interface IUpdateScopeOperation
    {
        Task<BaseResponse> ExecuteAsync(Uri scopesUri, ScopeResponse scope, string authorizationHeaderValue = null);
    }

    internal sealed class UpdateScopeOperation : IUpdateScopeOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UpdateScopeOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse> ExecuteAsync(Uri scopesUri, ScopeResponse scope, string authorizationHeaderValue = null)
        {
            if (scopesUri == null)
            {
                throw new ArgumentNullException(nameof(scopesUri));
            }

            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var serializedJson = JObject.FromObject(scope).ToString();
            var body = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = scopesUri,
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
