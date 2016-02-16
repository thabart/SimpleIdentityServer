using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Manager.Host.DTOs.Requests;
using SimpleIdentityServer.Manager.Host.DTOs.Responses;
using System;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    [Route(Constants.EndPoints.Jws)]
    public class JwsController : Controller
    {
        [HttpGet]
        public JwsInformationResponse GetJws(GetJwsRequest getJwsRequest)
        {
            if (getJwsRequest == null)
            {
                throw new ArgumentNullException(nameof(getJwsRequest));
            }

            return null;   
        }
    }
}
