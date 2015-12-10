using SimpleIdentityServer.Api.DTOs.Response;
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
            var identityServerException = actionExecutedContext.Exception as IdentityServerException;
            var identityServerExceptionWithState = actionExecutedContext.Exception as IdentityServerExceptionWithState;

            if (identityServerException != null)
            {
                var code = identityServerException.Code;
                var message = identityServerException.Message;
                var state = identityServerExceptionWithState == null
                    ? string.Empty
                    : identityServerExceptionWithState.State;
                _simpleIdentityServerEventSource.OpenIdFailure(code, message, state);
            }

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