using Newtonsoft.Json;
using SimpleIdentityServer.AccountFilter.Basic.Client.Results;
using SimpleIdentityServer.AccountFilter.Basic.Common.Requests;
using SimpleIdentityServer.AccountFilter.Basic.Common.Responses;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccountFilter.Basic.Client.Operations
{
    public interface IAddFilterOperation
    {
        Task<AddFilterResult> Execute(string requestUrl, AddFilterRequest addFilterRequest, string authorizationHeaderValue = null);
    }

    internal sealed class AddFilterOperation : IAddFilterOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AddFilterOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AddFilterResult> Execute(string requestUrl, AddFilterRequest addFilterRequest, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            if (addFilterRequest == null)
            {
                throw new ArgumentNullException(nameof(addFilterRequest));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var json = JsonConvert.SerializeObject(addFilterRequest, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json),
                RequestUri = new Uri(requestUrl)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
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
                return new AddFilterResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new AddFilterResult
            {
                Content = JsonConvert.DeserializeObject<AddFilterResponse>(content),
                HttpStatus = result.StatusCode
            };
        }
    }
}
