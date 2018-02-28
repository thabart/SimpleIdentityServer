using Newtonsoft.Json;
using SimpleIdentityServer.Configuration.Client.DTOs.Responses;
using SimpleIdentityServer.Configuration.Client.Factory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Client.Representation
{
    public interface IGetRepresentationsOperation
    {
        Task<IEnumerable<RepresentationResponse>> Execute(string representationUrl, string authorizationHeaderValue);
    }

    internal sealed class GetRepresentationsOperation : IGetRepresentationsOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetRepresentationsOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<RepresentationResponse>> Execute(string representationUrl, string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(representationUrl))
            {
                throw new ArgumentNullException(nameof(representationUrl));
            }

            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderValue));
            }

            Uri uri = null;
            if (!Uri.TryCreate(representationUrl, UriKind.Absolute, out uri))
            {
                throw new ArgumentException("the uri is not correct");
            }


            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri
            };
            request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            var httpResult = await httpClient.SendAsync(request).ConfigureAwait(false);
            httpResult.EnsureSuccessStatusCode();
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<List<RepresentationResponse>>(content);
        }
    }
}
