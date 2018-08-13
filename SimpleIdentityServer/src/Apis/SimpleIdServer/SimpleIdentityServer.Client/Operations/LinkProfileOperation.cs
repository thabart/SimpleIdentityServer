using Newtonsoft.Json;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Common.DTOs.Requests;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Operations
{
    public interface ILinkProfileOperation
    {

    }

    internal sealed class LinkProfileOperation : ILinkProfileOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LinkProfileOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task Execute(Uri requestUri, LinkProfileRequest addProfileRequest)
        {
            if (requestUri == null)
            {
                throw new ArgumentException(nameof(requestUri));
            }

            if(addProfileRequest == null)
            {
                throw new ArgumentNullException(nameof(addProfileRequest));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var json = JsonConvert.SerializeObject(addProfileRequest, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json),
                RequestUri = request
            };
        }
    }
}
