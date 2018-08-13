using Newtonsoft.Json;
using SimpleIdentityServer.Client.Results;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Operations
{
    public interface IGetProfilesOperation
    {
        Task<GetProfilesResult> Execute(string requestUrl, string currentSubject, string authorizationHeaderValue = null);
        Task<GetProfilesResult> Execute(string requestUrl, string authorizationHeaderValue = null);
    }

    internal sealed class GetProfilesOperation : IGetProfilesOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetProfilesOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task<GetProfilesResult> Execute(string requestUrl, string currentSubject, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            if (string.IsNullOrWhiteSpace(currentSubject))
            {
                throw new ArgumentNullException(nameof(currentSubject));
            }

            requestUrl += $"/{currentSubject}";
            return GetAll(requestUrl, authorizationHeaderValue);
        }

        public Task<GetProfilesResult> Execute(string requestUrl, string authorizationHeaderValue = null)
        {
            if(string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            requestUrl += "/.me";
            return GetAll(requestUrl, authorizationHeaderValue);
        }

        private async Task<GetProfilesResult> GetAll(string url, string authorizationValue = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            if (!string.IsNullOrWhiteSpace(authorizationValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationValue);
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var result = await httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetProfilesResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new GetProfilesResult
            {
                Content = JsonConvert.DeserializeObject<IEnumerable<ProfileResponse>>(content)
            };
        }
    }
}
