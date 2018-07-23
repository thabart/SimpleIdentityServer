using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Authenticate.SMS.Actions;
using SimpleIdentityServer.Authenticate.SMS.Common.Requests;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Core.Exceptions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.SMS.Controllers
{
    [Route(Constants.CodeController)]
    public class CodeController : Controller
    {
        private readonly ISmsAuthenticationOperation _smsAuthenticationOperation;

        public CodeController(ISmsAuthenticationOperation smsAuthenticationOperation)
        {
            _smsAuthenticationOperation = smsAuthenticationOperation;
        }

        /// <summary>
        /// Send the confirmation code to the phone number.
        /// </summary>
        /// <param name="confirmationCodeRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ConfirmationCodeRequest confirmationCodeRequest)
        {
            var checkResult = Check(confirmationCodeRequest);
            if (checkResult != null)
            {
                return checkResult;
            }

            IActionResult result = null;
            try
            {
                await _smsAuthenticationOperation.Execute(confirmationCodeRequest.PhoneNumber);
                result = new OkResult();
            }
            catch(IdentityServerException ex)
            {
                result = BuildError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch(Exception)
            {
                result = BuildError(Core.Errors.ErrorCodes.UnhandledExceptionCode, "unhandled exception occured please contact the administrator", HttpStatusCode.InternalServerError);
            }

            return result;
        }

        /// <summary>
        /// Check the parameter.
        /// </summary>
        /// <param name="confirmationCodeRequest"></param>
        /// <returns></returns>
        private IActionResult Check(ConfirmationCodeRequest confirmationCodeRequest)
        {
            if (confirmationCodeRequest == null)
            {
                return BuildError(Core.Errors.ErrorCodes.InvalidRequestCode, "no request", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(confirmationCodeRequest.PhoneNumber))
            {
                return BuildError(Core.Errors.ErrorCodes.InvalidRequestCode, $"parameter {SMS.Common.Constants.ConfirmationCodeRequestNames.PhoneNumber} is missing", HttpStatusCode.BadRequest);
            }

            return null;
        }

        /// <summary>
        /// Build the JSON error message.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorResponse
            {
                Error = code,
                ErrorDescription = message
            };
            return new JsonResult(error)
            {
                StatusCode = (int)statusCode
            };
        }
    }
}