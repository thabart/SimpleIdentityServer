using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Host.Controllers.Website;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Shell.Controllers
{
    [Area("Shell")]
    public class HomeController : BaseController
    {
        public HomeController(IAuthenticationService authenticationService) : base(authenticationService)
        {
        }

        #region Public methods

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await SetUser();
            return View();
        }

        #endregion
    }
}
