using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Host.Controllers.Website;
using SimpleIdentityServer.Shell.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Shell.Controllers
{
    [Area("Shell")]
    public class HomeController : BaseController
    {
        private readonly BasicShellOptions _basicShellOptions;

        public HomeController(IAuthenticationService authenticationService, AuthenticateOptions options, BasicShellOptions basicShellOptions) : base(authenticationService, options)
        {
            _basicShellOptions = basicShellOptions;
        }

        #region Public methods

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await SetUser();
            var viewModel = new List<UiModuleViewModel>();
            if (_basicShellOptions != null && _basicShellOptions.Descriptors != null)
            {
                foreach(var descriptor in _basicShellOptions.Descriptors)
                {
                    viewModel.Add(new UiModuleViewModel
                    {
                        IsAuthenticated = descriptor.IsAuthenticated,
                        Picture = descriptor.Picture,
                        RelativeUrl = descriptor.RelativeUrl,
                        Title = descriptor.Title
                    });
                }
            }

            return View(viewModel);
        }

        #endregion
    }
}
