using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Scopes
{
    public interface IGetAllScopesOperation
    {
        Task<GetAllScopesResponse> ExecuteAsync(Uri scopesUri, string authorizationHeaderValue = null);
    }

    internal sealed class GetAllScopesOperation : IGetAllScopesOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetAllScopesOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetAllScopesResponse> ExecuteAsync(Uri scopesUri, string authorizationHeaderValue = null)
        {
            if (scopesUri == null)
            {
                throw new ArgumentNullException(nameof(scopesUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = scopesUri
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
                return new GetAllScopesResponse(rec);
            }
            catch (Exception)
            {
                return new GetAllScopesResponse
                {
                    ContainsError = true
                };
            }

            var jArr = JArray.Parse(content);
            var result = new List<ScopeResponse>();
            foreach (JObject rec in jArr)
            {
                var scope = JsonConvert.DeserializeObject<ScopeResponse>(rec.ToString());
                if (scope != null)
                {
                    result.Add(scope);
                }
            }

            return new GetAllScopesResponse
            {
                Content = result
            };
        }
    }
}
