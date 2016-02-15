using System;
using SimpleIdentityServer.Host.DTOs.Response;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Core.Api.Registration;
using SimpleIdentityServer.Core.Results;

using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Host;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    [Route(Constants.EndPoints.Registration)]
    public class RegistrationController : Controller
    {
        private readonly IRegistrationActions _registerActions;

        public RegistrationController(IRegistrationActions registerActions)
        {
            _registerActions = registerActions;
        }

        [HttpPost]
        public RegistrationResponse Post(ClientResponse client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            
            return _registerActions.PostRegistration(client.ToParameter());
        }
    }
}