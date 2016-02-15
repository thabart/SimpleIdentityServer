using System;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using SwaggerLibrary.Helpers;
using SwaggerLibrary.Models;

namespace SwaggerLibrary
{
    public interface ISwaggerDocumentationParser
    {
        Task<SwaggerContract> GetContract();
    }

    public class SwaggerDocumentationParser : ISwaggerDocumentationParser
    {
        private readonly string _documentationUrl;

        private readonly IHttpClientHelper _httpClientHelper;

        public SwaggerDocumentationParser(IHttpClientHelper httpClientHelper)
        {
            _httpClientHelper = httpClientHelper;
        }

        public SwaggerDocumentationParser(string documentationUrl, IHttpClientHelper httpClientHelper)
        {
            _documentationUrl = documentationUrl;
            _httpClientHelper = httpClientHelper;
            if (!_documentationUrl.EndsWith("/"))
            {
                _documentationUrl += "/";
            }
        }

        public async Task<SwaggerContract> GetContract()
        {
            var httpClient = _httpClientHelper.GetHttpClient(_documentationUrl);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get
            };

            request.RequestUri = new Uri(_documentationUrl);
            var response = await httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return SwaggerContract.FromJObject(JObject.Parse(content));
        }
    }
}
