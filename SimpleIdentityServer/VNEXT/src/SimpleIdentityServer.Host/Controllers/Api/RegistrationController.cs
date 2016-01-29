using System;
using SimpleIdentityServer.Host.DTOs.Response;
using SimpleIdentityServer.Host.Extensions;
using SimpleIdentityServer.Core.Api.Registration;
using SimpleIdentityServer.Core.Results;

using Swashbuckle.Swagger.Annotations;

using Microsoft.AspNet.Mvc;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    public class RegistrationController : Controller
    {
        private readonly IRegistrationActions _registerActions;

        public RegistrationController(IRegistrationActions registerActions)
        {
            _registerActions = registerActions;
        }

        [SwaggerOperation("RegisterOperation")]
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