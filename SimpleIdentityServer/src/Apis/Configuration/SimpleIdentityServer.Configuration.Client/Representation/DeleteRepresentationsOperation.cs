using SimpleIdentityServer.Configuration.Client.Factory;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Client.Representation
{
    public interface IDeleteRepresentationsOperation
    {
        Task<bool> Execute(string representationUrl, string authorizationHeaderValue);
    }

    internal sealed class DeleteRepresentationsOperation : IDeleteRepresentationsOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteRepresentationsOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> Execute(string representationUrl, string authorizationHeaderValue)
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
                Method = HttpMethod.Delete,
                RequestUri = uri
            };
            request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            var httpResult = await httpClient.SendAsync(request).ConfigureAwait(false);
            httpResult.EnsureSuccessStatusCode();
            return true;
        }
    }
}
