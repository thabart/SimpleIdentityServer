using Microsoft.AspNet.Mvc;
using System.Collections.Generic;

namespace SimpleIdentityServer.TokenValidation.Host.Tests.Controllers
{
    [Route("values")]
    public class ValuesController : Controller
    {
        [HttpGet]
        public List<string> Get()
        {
            return new List<string>
            {
                "test"
            };
        }
    }
}
