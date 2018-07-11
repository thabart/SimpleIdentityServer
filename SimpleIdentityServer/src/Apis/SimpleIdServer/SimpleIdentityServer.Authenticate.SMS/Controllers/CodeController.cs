using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Authenticate.SMS.Actions;
using SimpleIdentityServer.Authenticate.SMS.Common.Requests;
using SimpleIdentityServer.Authenticate.SMS.Common.Responses;
using SimpleIdentityServer.Core.Exceptions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.SMS.Controllers
{
    [Route(Constants.CodeController)]
    public class CodeController : Controller
    {
        private readonly IGenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;
        private readonly ISmsAuthenticationOperation _smsAuthenticationOperation;

        public CodeController(IGenerateAndSendSmsCodeOperation generateAndSendSmsCodeOperation, ISmsAuthenticationOperation smsAuthenticationOperation)
        {
            _generateAndSendSmsCodeOperation = generateAndSendSmsCodeOperation;
            _smsAuthenticationOperation = smsAuthenticationOperation;
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ConfirmationCodeRequest confirmationCodeRequest)
        {
            Check(confirmationCodeRequest);
            IActionResult result = null;
            try
            {
                var resourceOwner = await _smsAuthenticationOperation.Execute(confirmationCodeRequest.PhoneNumber);
                await _generateAndSendSmsCodeOperation.Execute(confirmationCodeRequest.PhoneNumber);
                result = new OkResult();
            }
            catch(IdentityServerException ex)
            {
                var error = new ErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message
                };
                result = new JsonResult(error)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
            catch(Exception)
            {
                var error = new ErrorResponse
                {
                    Code = Core.Errors.ErrorCodes.UnhandledExceptionCode,
                    Message = "internal error"
                };
                result = new JsonResult(error)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            return result;
        }

        private void Check(ConfirmationCodeRequest confirmationCodeRequest)
        {
            if (confirmationCodeRequest == null)
            {
                throw new ArgumentNullException(nameof(confirmationCodeRequest));
            }

            if (string.IsNullOrWhiteSpace(confirmationCodeRequest.PhoneNumber))
            {
                throw new ArgumentNullException(nameof(confirmationCodeRequest.PhoneNumber));
            }
        }
    }
}