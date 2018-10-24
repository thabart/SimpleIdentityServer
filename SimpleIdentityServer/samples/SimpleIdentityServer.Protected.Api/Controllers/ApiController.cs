using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleIdentityServer.Protected.Api.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Index()
        {
            return Content("An API listing authors of docs.asp.net.");
        }
    }
}
