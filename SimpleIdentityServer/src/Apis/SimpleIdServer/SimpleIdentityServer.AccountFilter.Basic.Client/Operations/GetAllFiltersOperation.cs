using Newtonsoft.Json;
using SimpleIdentityServer.AccountFilter.Basic.Client.Results;
using SimpleIdentityServer.AccountFilter.Basic.Common.Responses;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccountFilter.Basic.Client.Operations
{
    public interface IGetAllFiltersOperation
    {
        Task<GetAllFiltersResult> Execute(string requestUrl, string authorizationHeaderValue = null);
    }

    internal sealed class GetAllFiltersOperation : IGetAllFiltersOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetAllFiltersOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetAllFiltersResult> Execute(string requestUrl, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(requestUrl)
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var result = await httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetAllFiltersResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new GetAllFiltersResult
            {
                Content = JsonConvert.DeserializeObject<IEnumerable<FilterResponse>>(content),
                HttpStatus = result.StatusCode
            };
        }
    }
}
