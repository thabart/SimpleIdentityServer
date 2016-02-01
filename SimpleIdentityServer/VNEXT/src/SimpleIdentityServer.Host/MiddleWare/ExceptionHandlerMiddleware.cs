using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.OptionsModel;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Host.DTOs.Response;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.MiddleWare
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ExceptionHandlerMiddlewareOptions _options;

        #region Constructor

        public ExceptionHandlerMiddleware(
            RequestDelegate next,
            ExceptionHandlerMiddlewareOptions options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next;
            _options = options;
        }

        #endregion

        #region Public methods

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var simpleIdentityServerEventSource = _options.SimpleIdentityServerEventSource;
                var identityServerException = exception as IdentityServerException;
                var identityServerExceptionWithState = exception as IdentityServerExceptionWithState;
                if (identityServerException == null)
                {
                    identityServerException = new IdentityServerException(ErrorCodes.UnhandledExceptionCode, exception.Message);
                }


                var code = identityServerException.Code;
                var message = identityServerException.Message;
                var state = identityServerExceptionWithState == null
                    ? string.Empty
                    : identityServerExceptionWithState.State;
                simpleIdentityServerEventSource.OpenIdFailure(code, message, state);
                context.Response.Clear();
                if (identityServerExceptionWithState != null)
                {
                    var errorResponseWithState = new ErrorResponseWithState
                    {
                        state = identityServerExceptionWithState.State
                    };

                    PopulateError(errorResponseWithState, identityServerExceptionWithState);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync(errorResponseWithState.SerializeWithDataContract());
                }
                else
                {
                    var error = new ErrorResponse();
                    PopulateError(error, identityServerException);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync(error.SerializeWithDataContract());
                }
            }
        }

        #endregion

        #region Private static methods
        
        private static void PopulateError(ErrorResponse errorResponse, IdentityServerException exception)
        {
            errorResponse.error = exception.Code;
            errorResponse.error_description = exception.Message;
        }

        #endregion
    }
}
