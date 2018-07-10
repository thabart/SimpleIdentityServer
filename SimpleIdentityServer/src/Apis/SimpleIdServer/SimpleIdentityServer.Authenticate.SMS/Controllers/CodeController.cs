using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Authenticate.SMS.Actions;
using SimpleIdentityServer.Authenticate.SMS.Common.Requests;
using SimpleIdentityServer.Authenticate.SMS.Common.Responses;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.SMS.Controllers
{
    [Route(Constants.CodeController)]
    public class CodeController : Controller
    {
        private readonly IGenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public CodeController(IGenerateAndSendSmsCodeOperation generateAndSendSmsCodeOperation, IResourceOwnerRepository resourceOwnerRepository)
        {
            _generateAndSendSmsCodeOperation = generateAndSendSmsCodeOperation;
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ConfirmationCodeRequest confirmationCodeRequest)
        {
            Check(confirmationCodeRequest);
            try
            {
                var resourceOwner = await _resourceOwnerRepository.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, confirmationCodeRequest.PhoneNumber);
                var subject = resourceOwner == null ? confirmationCodeRequest.PhoneNumber : resourceOwner.Id;
                await _generateAndSendSmsCodeOperation.Execute(confirmationCodeRequest.PhoneNumber, subject);
                if (resourceOwner == null)
                {
                    var newResourceOwner = new ResourceOwner
                    {
                        Id = confirmationCodeRequest.PhoneNumber,
                        CreateDateTime = DateTime.UtcNow,
                        UpdateDateTime = DateTime.UtcNow,
                        IsLocalAccount = true,
                        Password = PasswordHelper.ComputeHash(Guid.NewGuid().ToString()),
                        TwoFactorAuthentication = null,
                        Claims = new List<Claim>
                        {
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, confirmationCodeRequest.PhoneNumber),
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, confirmationCodeRequest.PhoneNumber),
                            new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()),
                        }
                    };

                    await _resourceOwnerRepository.InsertAsync(newResourceOwner);
                }

                return new OkResult();
            }
            catch(IdentityServerException ex)
            {
                var error = new ErrorResponse
                {
                    Code = ex.Code,
                    Message = ex.Message
                };
                var result = new JsonResult(error)
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
                var result = new JsonResult(error)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            return null;
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