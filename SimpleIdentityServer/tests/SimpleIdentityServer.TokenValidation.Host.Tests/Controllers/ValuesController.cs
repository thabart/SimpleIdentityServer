using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using System.Collections.Generic;

namespace SimpleIdentityServer.TokenValidation.Host.Tests.Controllers
{
    [Route("values")]
    public class ValuesController : Controller
    {
        [HttpGet]
        [Authorize("getValues")]
        public List<string> Get()
        {
            return new List<string>
            {
                "test"
            };
        }
    }
}
