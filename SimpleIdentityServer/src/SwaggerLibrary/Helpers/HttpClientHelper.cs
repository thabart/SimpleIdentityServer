using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SwaggerLibrary.Helpers
{
    public interface IHttpClientHelper
    {
        HttpClient GetHttpClient(string url);
    }

    public class HttpClientHelper : IHttpClientHelper
    {
        public HttpClient GetHttpClient(string url)
        {
            var httpClient = HttpClientFactory.Create();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
