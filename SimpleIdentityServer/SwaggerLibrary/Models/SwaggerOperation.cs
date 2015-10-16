using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace SwaggerLibrary.Models
{
    public class SwaggerOperation
    {
        private static Dictionary<string, HttpMethod> _httpRequests = new Dictionary<string, HttpMethod>
        {
            { "GET", HttpMethod.Get },
            { "POST", HttpMethod.Post },
            { "PUT", HttpMethod.Put },
            { "DELETE", HttpMethod.Delete }
        };

        public HttpMethod HttpRequest { get; set; }

        public string OperationId { get; set; }

        public List<SwaggerParameter> Parameters { get; set; } 

        public static SwaggerOperation FromJObject(JProperty obj)
        {
            var httpMethod = obj.Name.ToUpperInvariant();
            if (!_httpRequests.ContainsKey(httpMethod))
            {
                throw new NotSupportedException(string.Format("The httpMethod {0} is not supported", httpMethod));
            }

            var result = new SwaggerOperation
            {
                OperationId = obj.Value["operationId"].ToString(),
                HttpRequest = _httpRequests[httpMethod]
            };

            result.Parameters = new List<SwaggerParameter>();
            var parameters = obj.Value["parameters"];
            if (parameters != null && parameters.Any())
            {
                foreach (var parameter in parameters)
                {
                    result.Parameters.Add(SwaggerParameter.FromJObject((JObject)parameter));
                }
            }

            return result;
        }
    }
}
