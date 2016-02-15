using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Host;
using SimpleIdentityServer.Host.ViewModels;

namespace SimpleIdentityServer.Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly SwaggerOptions _swaggerOptions;
        
        #region Constructor
        
        public HomeController(SwaggerOptions swaggerOptions) 
        {
            _swaggerOptions = swaggerOptions;
        }
        
        #endregion
        
        #region Public methods
        
        [HttpGet]
        public ActionResult Index() 
        {
            var viewModel = new HomeViewModel 
            {
                IsSwaggerEnabled = _swaggerOptions.IsSwaggerEnabled
            };
            
            return View(viewModel);    
        }
        
        #endregion
    }
}