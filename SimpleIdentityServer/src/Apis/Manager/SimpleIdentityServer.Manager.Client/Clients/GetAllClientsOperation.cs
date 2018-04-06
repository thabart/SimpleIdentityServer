using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.Manager.Client.Factories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Client.Clients
{
    public interface IGetAllClientsOperation
    {
        Task<GetAllClientResponse> ExecuteAsync(Uri clientsUri, string authorizationHeaderValue = null);
    }

    internal sealed class GetAllClientsOperation : IGetAllClientsOperation
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GetAllClientsOperation(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetAllClientResponse> ExecuteAsync(Uri clientsUri, string authorizationHeaderValue = null)
        {
            if (clientsUri == null)
            {
                throw new ArgumentNullException(nameof(clientsUri));
            }

            var httpClient = _httpClientFactory.GetHttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = clientsUri
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
            catch(HttpRequestException)
            {
                ErrorResponse resp = null;
                var rec = JObject.Parse(content);
                if (rec != null)
                {
                    resp = ErrorResponse.ToError(rec);
                }

                return new GetAllClientResponse(resp);
            }
            catch(Exception)
            {
                return new GetAllClientResponse
                {
                    ContainsError = true
                };
            }

            var jArr = JArray.Parse(content);
            var result = new List<OpenIdClientResponse>();
            foreach (JObject rec in jArr)
            {
                result.Add(OpenIdClientResponse.ToClient(rec));
            }

            return new GetAllClientResponse
            {
                Content = result
            };
        }
    }
}
