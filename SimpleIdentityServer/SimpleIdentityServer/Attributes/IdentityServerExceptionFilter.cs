using SimpleIdentityServer.Api.DTOs.Response;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;

using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Api.Attributes
{
    public class IdentityServerExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        public IdentityServerExceptionFilter(ISimpleIdentityServerEventSource simpleIdentityServerEventSource)
        {
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
        }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var exception = actionExecutedContext.Exception;
            var identityServerException = actionExecutedContext.Exception as IdentityServerException;
            var identityServerExceptionWithState = actionExecutedContext.Exception as IdentityServerExceptionWithState;
            if (identityServerException == null)
            {
                identityServerException = new IdentityServerException(ErrorCodes.UnhandledExceptionCode, exception.Message);
            }


            var code = identityServerException.Code;
            var message = identityServerException.Message;
            var state = identityServerExceptionWithState == null
                ? string.Empty
                : identityServerExceptionWithState.State;
            _simpleIdentityServerEventSource.OpenIdFailure(code, message, state);
            HttpResponseMessage responseMessage;
            if (identityServerExceptionWithState != null)
            {
                var errorResponseWithState = new ErrorResponseWithState
                {
                    state = identityServerExceptionWithState.State
                };

                PopulateError(errorResponseWithState, identityServerExceptionWithState);
                responseMessage = actionExecutedContext.Request.CreateResponse(HttpStatusCode.BadRequest,
                    errorResponseWithState);
            }
            else
            {
                var error = new ErrorResponse();
                PopulateError(error, identityServerException);
                responseMessage = actionExecutedContext.Request.CreateResponse(HttpStatusCode.BadRequest, error);
            }

            if (responseMessage != null)
            {
                actionExecutedContext.Response = responseMessage;
            }
        }

        private static void PopulateError(ErrorResponse errorResponse, IdentityServerException exception)
        {
            errorResponse.error = exception.Code;
            errorResponse.error_description = exception.Message;
        }
    }
}