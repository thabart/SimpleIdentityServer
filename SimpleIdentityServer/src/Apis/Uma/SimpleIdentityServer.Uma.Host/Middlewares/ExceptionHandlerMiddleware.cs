#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.Http;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Host.Middlewares
{
    internal class ExceptionHandlerMiddleware
    {
        private const string UnhandledExceptionCode = "unhandled_error";
        private readonly RequestDelegate _next;
        private readonly ExceptionHandlerMiddlewareOptions _options;

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

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var identityServerException = exception as BaseUmaException;
                if (identityServerException == null)
                {
                    identityServerException = new BaseUmaException(UnhandledExceptionCode, exception.Message);
                }

                _options.UmaEventSource.Failure(identityServerException);
                var code = identityServerException.Code;
                var message = identityServerException.Message;
                var error = new ErrorResponse();
                PopulateError(error, identityServerException);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                var serializedError = error.SerializeWithDataContract();
                await context.Response.WriteAsync(serializedError);
            }
        }

        private static void PopulateError(ErrorResponse errorResponse, BaseUmaException exception)
        {
            errorResponse.Error = exception.Code;
            errorResponse.ErrorDescription = exception.Message;
        }
    }
}
