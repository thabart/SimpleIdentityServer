using System.Collections.Generic;
using Microsoft.AspNet.Mvc;


namespace SimpleIdentityServer.Host.Controllers {
    
    [Route("token")]
    public class ValuesController : Controller 
    {
        
        public IEnumerable<string> All() {
            return new List<string> 
            {
                "coucou"
            };
        }
        
    }
}