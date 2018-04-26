using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;

namespace SimpleIdentityServer.ResourceManager.API.Host.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult GetError(this Controller controller, string description, HttpStatusCode httpStatusCode)
        {
            return GetError(controller, Constants.ErrorCodes.InternalError, description, httpStatusCode);
        }

        public static IActionResult GetError(this Controller controller, string code, string description, HttpStatusCode httpStatusCode)
        {
            var jObj = new JObject();
            jObj.Add(Constants.ErrorDtoNames.Code, code);
            jObj.Add(Constants.ErrorDtoNames.Message, description);
            return new JsonResult(jObj)
            {
                StatusCode = (int)httpStatusCode
            };
        }
    }
}
