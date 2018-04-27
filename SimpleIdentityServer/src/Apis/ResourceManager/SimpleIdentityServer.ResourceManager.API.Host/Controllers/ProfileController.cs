using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.ResourceManager.API.Host.DTOs;
using SimpleIdentityServer.ResourceManager.API.Host.Extensions;
using SimpleIdentityServer.ResourceManager.Core.Api.Profile;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ProfileController)]
    public class ProfileController : Controller
    {
        private readonly IProfileActions _profileActions;

        public ProfileController(IProfileActions profileActions)
        {
            _profileActions = profileActions;
        }

        [Authorize("connected")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var subject = User.GetSubject();
            try
            {
                var profile = await _profileActions.GetProfile(subject);
                if (profile == null)
                {
                    return new NotFoundResult();
                }

                return new OkObjectResult(profile.ToDto());
            }
            catch(Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [Authorize("connected")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ProfileResponse profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            var subject = User.GetSubject();
            try
            {
                var parameter = profile.ToParameter();
                parameter.Subject = subject;
                if (!await _profileActions.Update(parameter))
                {
                    return this.GetError(Constants.Errors.ErrUpdateProfile, HttpStatusCode.InternalServerError);
                }

                return Ok();
            }
            catch(Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
