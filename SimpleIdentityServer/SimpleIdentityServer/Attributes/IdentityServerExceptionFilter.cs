using SimpleIdentityServer.Api.DTOs.Response;
using SimpleIdentityServer.Core.Exceptions;

using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace SimpleIdentityServer.Api.Attributes
{
    public class IdentityServerExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var identityServerExceptionWithState = actionExecutedContext.Exception as IdentityServerExceptionWithState;
            if (identityServerExceptionWithState != null)
            {
                var error = new ErrorResponseWithState
                {
                    state = identityServerExceptionWithState.State
                };

                PopulateError(error, identityServerExceptionWithState);
                var response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.BadRequest, error);
                actionExecutedContext.Response = response;
                return;
            }

            var identityServerException = actionExecutedContext.Exception as IdentityServerException;
            if (identityServerException != null)
            {
                var error = new ErrorResponse();
                PopulateError(error, identityServerException);
                var response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.BadRequest, error);
                actionExecutedContext.Response = response;
            }
        }

        private static void PopulateError(ErrorResponse errorResponse, IdentityServerException exception)
        {
            errorResponse.error = exception.Code;
            errorResponse.error_description = exception.Message;
        }
    }
}