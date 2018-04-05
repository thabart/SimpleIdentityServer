using Newtonsoft.Json.Linq;
using System;

namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public class ErrorResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }

        public static ErrorResponse ToError(JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var response = new ErrorResponse();
            JToken jCode;
            if (jObj.TryGetValue("code", out jCode))
            {
                response.Message = jCode.ToString();
            }

            JToken jMessage;
            if (jObj.TryGetValue("message", out jMessage))
            {
                response.Message = jMessage.ToString();
            }

            return response;
        }
    }
}
