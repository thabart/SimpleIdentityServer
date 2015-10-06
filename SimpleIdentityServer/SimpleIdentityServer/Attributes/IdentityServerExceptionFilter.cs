using SimpleIdentityServer.Api.DTOs.Response;
using SimpleIdentityServer.Core.Errors;

using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace SimpleIdentityServer.Api.Attributes
{
    public class IdentityServerExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var exception = actionExecutedContext.Exception as IdentityServerException;
            if (exception != null)
            {
                var error = new ErrorResponse
                {
                    error = exception.Code,
                    error_description = exception.Message
                };

                var response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.BadRequest, error);
                actionExecutedContext.Response = response;
            }
        }
    }
}