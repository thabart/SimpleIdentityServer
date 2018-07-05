using Newtonsoft.Json;
using SimpleIdentityServer.Authenticate.SMS.Client.Factories;
using SimpleIdentityServer.Authenticate.SMS.Common.Requests;
using SimpleIdentityServer.Authenticate.SMS.Common.Responses;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.SMS.Client
{
    public interface ISendSmsOperation
    {
        Task<ErrorResponse> Execute(Uri requestUri, ConfirmationCodeRequest request, string authorizationValue = null);
    }

    internal sealed class SendSmsOperation : ISendSmsOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SendSmsOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ErrorResponse> Execute(Uri requestUri, ConfirmationCodeRequest request, string authorizationValue = null)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            
            var httpClient = _httpClientFactory.GetHttpClient();
            var json = JsonConvert.SerializeObject(request);
            var req = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json),
                RequestUri = requestUri
            };
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(authorizationValue))
            {
                req.Headers.Add("Authorization", "Basic " + authorizationValue);
            }

            var result = await httpClient.SendAsync(req);
            var content = await result.Content.ReadAsStringAsync();
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch
            {
                return JsonConvert.DeserializeObject<ErrorResponse>(content);
            }

            return null;
        }
    }
}
