using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SwaggerLibrary.Helpers;
using SwaggerLibrary.Models;

namespace SwaggerLibrary
{
    public interface IWebApiRequestor
    {
        WebApiRequestor CreateRequest(string operationId, SwaggerContract contract);

        WebApiRequestor PassParameters(object parameter);

        Task<HttpResponseMessage> ExecuteRequest();
    }

    public class WebApiRequestor : IWebApiRequestor
    {
        private readonly string _host;

        private readonly IHttpClientHelper _httpClientHelper;

        private HttpRequestMessage _requestMessage;

        private SwaggerOperation _swaggerOperation;

        public WebApiRequestor(IHttpClientHelper httpClientHelper)
        {
            _httpClientHelper = httpClientHelper;
        }

        public WebApiRequestor(string host, IHttpClientHelper httpClientHelper)
        {
            _host = host;
            _httpClientHelper = httpClientHelper;
        }

        public WebApiRequestor CreateRequest(string operationId, SwaggerContract contract)
        {
            var operationInformations = GetInformationsAboutOperation(operationId, contract);
            var path = _host;
            if (path.EndsWith("/"))
            {
                path = path.Remove(path.Length - 1);
            }

            path = path + operationInformations.Item1;
            var httpRequest = operationInformations.Item2.HttpRequest;
            _swaggerOperation = operationInformations.Item2;
            _requestMessage = new HttpRequestMessage
            {
                Method = httpRequest,
                RequestUri = new Uri(path)
            };

            return this;
        }

        public WebApiRequestor PassParameters(object parameter)
        {
            if (_requestMessage == null || _swaggerOperation == null)
            {
                throw new NullReferenceException("the request must be created");
            }
            
            // TODO : This class must be modified later on to support multiple parameters.
            var parameterDefinition = _swaggerOperation.Parameters.FirstOrDefault();
            if (parameterDefinition == null)
            {
                throw new NotSupportedException("the operation contains no parameter, not possible to pass parameter");
            }

            switch (parameterDefinition.In)
            {
                case SwaggerParameterInEnum.Query:
                case SwaggerParameterInEnum.Path:
                    var uri = _requestMessage.RequestUri;
                    uri = uri.BuildUri(parameter);
                    _requestMessage.RequestUri = uri;
                    break;
                case SwaggerParameterInEnum.Body:
                    _requestMessage.Content = new ObjectContent(parameter.GetType(), parameter, new JsonMediaTypeFormatter());
                    _requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    break;
            }
            return this;
        }

        public Task<HttpResponseMessage> ExecuteRequest()
        {
            if (_requestMessage == null)
            {
                throw new NullReferenceException("cannot execute a none existing HTTP request");
            }

            var httpClient = _httpClientHelper.GetHttpClient(_host);
            return httpClient.SendAsync(_requestMessage);
        }

        private Tuple<string, SwaggerOperation> GetInformationsAboutOperation(string operationId, SwaggerContract contract)
        {
            if (contract == null || contract.Paths == null || !contract.Paths.Any())
            {
                throw new ArgumentException("A path must be specified in the contract");
            }

            foreach (var path in contract.Paths)
            {
                var operation = path.Operations.FirstOrDefault(o => !string.IsNullOrEmpty(o.OperationId) 
                    && o.OperationId.ToUpperInvariant() == operationId.ToUpperInvariant());
                if (operation != null)
                {
                    return new Tuple<string, SwaggerOperation>(path.Path, operation);
                }
            }

            return null;
        }
    }
}
