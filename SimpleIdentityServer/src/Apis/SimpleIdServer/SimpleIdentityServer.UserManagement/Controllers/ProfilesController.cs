using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Common.Dtos.Responses;
using SimpleIdentityServer.Core.Api.Profile;
using SimpleIdentityServer.Core.Common.DTOs.Requests;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.UserManagement.Extensions;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.UserManagement.Controllers
{
    [Route("profiles")]
    public class ProfilesController : Controller
    {
        private readonly IProfileActions _profileActions;

        public ProfilesController(IProfileActions profileActions)
        {
            _profileActions = profileActions;
        }

        #region Public methods

        [HttpGet(".me")]
        [Authorize("connected_user")]
        public Task<IActionResult> GetProfiles()
        {
            if (User == null || User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Task.FromResult((IActionResult)new StatusCodeResult((int)HttpStatusCode.Unauthorized));
            }

            var subject = User.GetSubject();
            return GetProfiles(subject);
        }

        [HttpGet("{subject}")]
        [Authorize("manage_profile")]
        public async Task<IActionResult> GetProfiles(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return BuildMissingParameter(nameof(subject));
            }

            var profiles = await _profileActions.GetProfiles(subject);
            return new OkObjectResult(profiles.Select(p => p.ToDto()));
        }

        [HttpPost(".me")]
        [Authorize("connected_user")]
        public Task<IActionResult> AddProfile([FromBody] LinkProfileRequest linkProfileRequest)
        {
            return AddProfile(User.GetSubject(), linkProfileRequest);
        }

        [HttpPost("{subject}")]
        [Authorize("manage_profile")]
        public async Task<IActionResult> AddProfile(string subject, [FromBody] LinkProfileRequest linkProfileRequest)
        {
            if(string.IsNullOrWhiteSpace(subject))
            {
                return BuildMissingParameter(nameof(subject));
            }

            if (linkProfileRequest == null)
            {
                return BuildMissingParameter(nameof(linkProfileRequest));
            }

            await _profileActions.Link(subject, linkProfileRequest.UserId, linkProfileRequest.Issuer, linkProfileRequest.Force);
            return new NoContentResult();
        }
        
        [HttpDelete(".me/{externalId}")]
        [Authorize("connected_user")]
        public Task<IActionResult> RemoveProfile(string externalId)
        {
            return RemoveProfile(User.GetSubject(), externalId);
        }

        [HttpDelete("{subject}/{externalId}")]
        [Authorize("manage_profile")]
        public async Task<IActionResult> RemoveProfile(string subject, string externalId)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return BuildMissingParameter(nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(externalId))
            {
                return BuildMissingParameter(nameof(externalId));
            }

            await _profileActions.Unlink(subject, externalId);
            return new NoContentResult();
        }

        #endregion

        private static IActionResult BuildMissingParameter(string parameterName)
        {
            var error = new ErrorResponse
            {
                Error = ErrorCodes.InvalidRequestCode,
                ErrorDescription = string.Format(ErrorDescriptions.MissingParameter, parameterName)
            };

            return new JsonResult(error)
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }
    }
}
